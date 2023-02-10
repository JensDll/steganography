using System.IO.Pipelines;
using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.Extensions;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace WebApi.Features.Codec.EncodeBinary;

public class EncodeBinaryRequest
{
    private PipeWriter PipeWriter { get; init; } = null!;
    private MultipartReader MultipartReader { get; init; } = null!;

    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required PipeReader PipeReader { get; init; }

    public static async ValueTask<EncodeBinaryRequest?> BindAsync(HttpContext context)
    {
        MultipartReader? multipartReader = MultipartReader.Create(context);

        if (multipartReader is null)
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

        return new EncodeBinaryRequest
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            PipeReader = pipe.Reader,
            MultipartReader = multipartReader,
            PipeWriter = pipe.Writer
        };
    }

    public async Task<int?> FillPipeAsync(
        AesCounterMode aes,
        List<string> validationErrors,
        CancellationToken cancellationToken)
    {
        int messageLength = 0;

        while (await MultipartReader.ReadNextSectionAsync(cancellationToken) is { } nextSection)
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

            nextSection = await MultipartReader.ReadNextSectionAsync(cancellationToken);

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
                validationErrors.Add("The filename cannot be longer than 256 bytes");
                PipeReader.CancelPendingRead();
                return null;
            }

            int sizeHint = 6 + filenameSize;
            messageLength += sizeHint + fileLength;

            if (messageLength > CoverImageCapacity)
            {
                validationErrors.Add("The message is too long for the cover image");
                PipeReader.CancelPendingRead();
                return null;
            }

            Memory<byte> buffer = PipeWriter.GetMemory(sizeHint);

            // Write the file length (4-byte)
            BitConverter.TryWriteBytes(buffer.Span, fileLength);
            // Write the file name size (2-byte)
            BitConverter.TryWriteBytes(buffer.Span[4..], (short)filenameSize);
            // Write the file name (max 256-byte)
            Encoding.UTF8.GetBytes(fileSection.FileName, buffer.Span[6..]);

            aes.Transform(buffer.Span[..sizeHint], buffer.Span);

            PipeWriter.Advance(sizeHint);

            await PipeWriter.FlushAsync(cancellationToken);

            while (true)
            {
                buffer = PipeWriter.GetMemory();
                int bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                aes.Transform(buffer.Span[..bytesRead], buffer.Span);

                PipeWriter.Advance(bytesRead);

                FlushResult result = await PipeWriter.FlushAsync(cancellationToken);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

        await PipeWriter.CompleteAsync();

        return messageLength;
    }
}
