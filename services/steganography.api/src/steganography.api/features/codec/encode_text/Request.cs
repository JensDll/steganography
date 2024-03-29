﻿using System.IO.Pipelines;
using Microsoft.AspNetCore.WebUtilities;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using steganography.api.extensions;
using steganography.domain;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace steganography.api.features.codec;

public class EncodeTextRequest
{
    private PipeWriter PipeWriter { get; init; } = null!;
    private MultipartReader MultipartReader { get; init; } = null!;

    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required PipeReader PipeReader { get; init; }

    public static async ValueTask<EncodeTextRequest?> BindAsync(HttpContext context)
    {
        DecodeEndpoint endpoint = context.RequestServices.GetRequiredService<DecodeEndpoint>();

        MultipartReader multipartReader = new(context, endpoint);

        if (endpoint.HasValidationError)
        {
            return null;
        }

        CancellationToken cancellationToken = context.RequestAborted;

        NextSection? nextSection = await multipartReader.ReadNextSectionAsync(cancellationToken);
        FileMultipartSection? fileSection = nextSection?.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return null;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync(cancellationToken);

        if (coverImage is null)
        {
            return null;
        }

        context.Response.RegisterForDispose(coverImage);

        Pipe pipe = new();

        return new EncodeTextRequest
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            PipeReader = pipe.Reader,
            MultipartReader = multipartReader,
            PipeWriter = pipe.Writer
        };
    }

    public async Task<int?> FillPipeAsync(AesCounterMode aes,
        ICollection<string> validationErrors,
        CancellationToken cancellationToken)
    {
        NextSection? nextSection = await MultipartReader.ReadNextSectionAsync(cancellationToken);
        FormMultipartSection? messageSection = nextSection?.AsFormSection("message");

        if (messageSection is null)
        {
            validationErrors.Add("The request does not contain a message");
            PipeReader.CancelPendingRead();
            return null;
        }

        int messageLength = 0;

        while (true)
        {
            Memory<byte> buffer = PipeWriter.GetMemory();

            int bytesRead = await messageSection.Section.Body.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            messageLength += bytesRead;

            if (messageLength > CoverImageCapacity)
            {
                validationErrors.Add("The message is too long for the cover image");
                PipeReader.CancelPendingRead();
                return null;
            }

            aes.Transform(buffer.Span[..bytesRead], buffer.Span);
            PipeWriter.Advance(bytesRead);

            FlushResult result = await PipeWriter.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await PipeWriter.CompleteAsync();

        return messageLength;
    }
}
