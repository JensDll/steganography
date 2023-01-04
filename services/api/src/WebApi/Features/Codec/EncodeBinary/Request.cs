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

    public async Task<int?> FillPipeAsync(AesCounterMode aes,
        Action<string> addValidationError,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FormFile>? files = await MultipartReader.ReadFilesBufferedAsync(cancellationToken);

        if (files is null)
        {
            PipeReader.CancelPendingRead();
            await PipeWriter.CompleteAsync();
            return null;
        }

        int messageLength = 0;
        int[] sizeHints = new int[files.Count];

        for (int i = 0; i < files.Count; ++i)
        {
            FormFile file = files[i];

            int filenameSize = Encoding.UTF8.GetByteCount(file.FileName);

            if (filenameSize > 256)
            {
                addValidationError("The filename cannot be longer than 256 bytes");
                PipeReader.CancelPendingRead();
                await PipeWriter.CompleteAsync();
                return null;
            }

            int sizeHint = 6 + filenameSize;
            messageLength += sizeHint + (int)file.Length;

            if (messageLength > CoverImageCapacity)
            {
                addValidationError("The message is too long for the cover image");
                PipeReader.CancelPendingRead();
                await PipeWriter.CompleteAsync();
                return null;
            }

            sizeHints[i] = sizeHint;
        }

        for (int i = 0; i < files.Count; ++i)
        {
            FormFile file = files[i];
            int sizeHint = sizeHints[i];

            Memory<byte> buffer = PipeWriter.GetMemory(sizeHint);
            // Write the file length (4-byte)
            BitConverter.TryWriteBytes(buffer.Span, file.Length);
            // Write the file name size (2-byte)
            BitConverter.TryWriteBytes(buffer.Span[4..], (short)sizeHint - 6);
            // Write the file name (max 256-byte)
            Encoding.UTF8.GetBytes(file.FileName, buffer.Span[6..]);

            aes.Transform(buffer.Span[..sizeHint], buffer.Span);
            PipeWriter.Advance(sizeHint);

            await PipeWriter.FlushAsync(cancellationToken);
        }

        foreach (FormFile file in files)
        {
            Stream fileStream = file.OpenReadStream();

            while (true)
            {
                Memory<byte> buffer = PipeWriter.GetMemory();
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
