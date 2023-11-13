using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using steganography.api.extensions;
using steganography.domain;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace steganography.api.features.codec;

public class EncodeBinaryRequest
{
    private readonly MultipartReader _multipartReader;
    private readonly PipeWriter _pipeWriter;

    private EncodeBinaryRequest(PipeWriter pipeWriter, MultipartReader multipartReader)
    {
        _pipeWriter = pipeWriter;
        _multipartReader = multipartReader;
    }

    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required PipeReader PipeReader { get; init; }

    public static async ValueTask<EncodeBinaryRequest?> BindAsync(HttpContext context)
    {
        EncodeBinaryEndpoint endpoint = context.RequestServices.GetRequiredService<EncodeBinaryEndpoint>();

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

        return new EncodeBinaryRequest(pipe.Writer, multipartReader)
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            PipeReader = pipe.Reader
        };
    }

    public async Task<int?> FillPipeAsync(
        AesCounterMode aes,
        MinimalApiBuilderEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        int messageLength = 0;

        while (await _multipartReader.ReadNextSectionAsync(cancellationToken) is { } nextSection)
        {
            FormMultipartSection? lengthSection = nextSection.AsFormSection();

            if (lengthSection is null)
            {
                PipeReader.CancelPendingRead();
                return null;
            }

            string length = await lengthSection.GetValueAsync(cancellationToken);

            if (!int.TryParse(length, out int fileLength))
            {
                PipeReader.CancelPendingRead();
                return null;
            }

            nextSection = await _multipartReader.ReadNextSectionAsync(cancellationToken);

            FileMultipartSection? fileSection = nextSection?.AsFileSection();

            if (fileSection is null)
            {
                PipeReader.CancelPendingRead();
                return null;
            }

            Stream? fileStream = fileSection.FileStream;

            if (fileStream is null)
            {
                PipeReader.CancelPendingRead();
                return null;
            }

            int filenameSize = Encoding.UTF8.GetByteCount(fileSection.FileName);

            if (filenameSize > 256)
            {
                endpoint.AddValidationError("file", "The filename cannot be longer than 256 bytes.");
                PipeReader.CancelPendingRead();
                return null;
            }

            int sizeHint = 6 + filenameSize;
            messageLength += sizeHint + fileLength;

            if (messageLength > CoverImageCapacity)
            {
                endpoint.AddValidationError("message", "The message is too long for the cover image.");
                PipeReader.CancelPendingRead();
                return null;
            }

            Memory<byte> buffer = _pipeWriter.GetMemory(sizeHint);

            // Write the file length (4-byte)
            BitConverter.TryWriteBytes(buffer.Span, fileLength);
            // Write the file name size (2-byte)
            BitConverter.TryWriteBytes(buffer.Span[4..], (short)filenameSize);
            // Write the file name (max 256-byte)
            Encoding.UTF8.GetBytes(fileSection.FileName, buffer.Span[6..]);

            aes.Transform(buffer.Span[..sizeHint], buffer.Span);

            _pipeWriter.Advance(sizeHint);

            await _pipeWriter.FlushAsync(cancellationToken);

            while (true)
            {
                buffer = _pipeWriter.GetMemory();
                int bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                aes.Transform(buffer.Span[..bytesRead], buffer.Span);

                _pipeWriter.Advance(bytesRead);

                FlushResult result = await _pipeWriter.FlushAsync(cancellationToken);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

        await _pipeWriter.CompleteAsync();

        return messageLength;
    }
}
