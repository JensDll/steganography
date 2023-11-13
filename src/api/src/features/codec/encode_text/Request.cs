using System.IO.Pipelines;
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
    private readonly MultipartReader _multipartReader;
    private readonly PipeWriter _pipeWriter;

    private EncodeTextRequest(PipeWriter pipeWriter, MultipartReader multipartReader)
    {
        _pipeWriter = pipeWriter;
        _multipartReader = multipartReader;
    }

    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required PipeReader PipeReader { get; init; }

    public static async ValueTask<EncodeTextRequest?> BindAsync(HttpContext context)
    {
        EncodeTextEndpoint endpoint = context.RequestServices.GetRequiredService<EncodeTextEndpoint>();

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

        return new EncodeTextRequest(pipe.Writer, multipartReader)
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            PipeReader = pipe.Reader
        };
    }

    public async Task<int?> FillPipeAsync(AesCounterMode aes,
        MinimalApiBuilderEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        NextSection? nextSection = await _multipartReader.ReadNextSectionAsync(cancellationToken);
        FormMultipartSection? messageSection = nextSection?.AsFormSection("message");

        if (messageSection is null)
        {
            endpoint.AddValidationError("message", "The request does not contain a message.");
            PipeReader.CancelPendingRead();
            return null;
        }

        int messageLength = 0;

        while (true)
        {
            Memory<byte> buffer = _pipeWriter.GetMemory();

            int bytesRead = await messageSection.Section.Body.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            messageLength += bytesRead;

            if (messageLength > CoverImageCapacity)
            {
                endpoint.AddValidationError("message", "The message is too long for the cover image.");
                PipeReader.CancelPendingRead();
                return null;
            }

            aes.Transform(buffer.Span[..bytesRead], buffer.Span);
            _pipeWriter.Advance(bytesRead);

            FlushResult result = await _pipeWriter.FlushAsync(cancellationToken);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await _pipeWriter.CompleteAsync();

        return messageLength;
    }
}
