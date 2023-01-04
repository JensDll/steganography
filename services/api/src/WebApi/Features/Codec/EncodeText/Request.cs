using System.IO.Pipelines;
using Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.Extensions;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace WebApi.Features.Codec.EncodeText;

public class EncodeTextRequest
{
    private PipeWriter PipeWriter { get; init; } = null!;
    private MultipartReader MultipartReader { get; init; } = null!;

    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required PipeReader PipeReader { get; init; }

    public static async ValueTask<EncodeTextRequest?> BindAsync(HttpContext context)
    {
        MultipartReader multipartReader;

        try
        {
            multipartReader = new MultipartReader(context);
        }
        catch
        {
            return null;
        }

        NextSection? nextSection = await multipartReader.ReadNextSectionAsync();
        FileMultipartSection? fileSection = nextSection?.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return null;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync();

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
        Action<string> addValidationError,
        CancellationToken cancellationToken)
    {
        NextSection? nextSection = await MultipartReader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            addValidationError("The request does not contain a message");
            PipeReader.CancelPendingRead();
            await PipeWriter.CompleteAsync();
            return null;
        }

        FormMultipartSection? formSection = nextSection.AsFormSection("message");

        if (formSection is null)
        {
            PipeReader.CancelPendingRead();
            await PipeWriter.CompleteAsync();
            return null;
        }

        int messageLength = 0;

        while (true)
        {
            Memory<byte> buffer = PipeWriter.GetMemory();

            int bytesRead = await formSection.Section.Body.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            messageLength += bytesRead;

            if (messageLength > CoverImageCapacity)
            {
                addValidationError("The message is too long for the cover image");
                PipeReader.CancelPendingRead();
                PipeWriter.CancelPendingFlush();
                await PipeWriter.FlushAsync(cancellationToken);
                await PipeWriter.CompleteAsync();
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
