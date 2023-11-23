using System.IO.Compression;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Text;
using Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Steganography.Extensions;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace Steganography.Features.Codec;

internal sealed partial class EncodeBinaryEndpoint : MinimalApiBuilderEndpoint
{
    public static async Task<Results<EmptyHttpResult, ValidationProblem>> Handle(
        EncodeBinaryRequest request,
        [FromServices] EncodeBinaryEndpoint endpoint,
        [FromServices] IKeyService keyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        int? messageLength;

        using AesCounterMode aes = new();
        using ImageEncoder imageEncoder = new(request.CoverImage, seed);

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes, endpoint, cancellationToken);
            Task reading = imageEncoder.EncodeAsync(request.PipeReader, cancellationToken);
            await Task.WhenAll(writing, reading);
            messageLength = writing.Result;
        }
        catch (OperationCanceledException)
        {
            return TypedResults.Empty;
        }
        catch (InvalidOperationException)
        {
            return TypedResults.ValidationProblem(endpoint.ValidationErrors, title: "Encoding failed.");
        }

        if (!messageLength.HasValue)
        {
            return TypedResults.ValidationProblem(endpoint.ValidationErrors, title: "Encoding failed.");
        }

        string base64Key = keyService.ToBase64String(MessageType.Binary, seed,
            messageLength.Value, aes.Key, aes.InitializationValue);

        ContentDispositionHeaderValue contentDisposition = new("attachment");
        contentDisposition.SetHttpFileName("secret.zip");
        httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
        httpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(httpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

        ZipArchiveEntry coverImageEntry = archive.CreateEntry("image.png", CompressionLevel.Fastest);

        try
        {
            await using Stream coverImageStream = coverImageEntry.Open();
            await request.CoverImage.SaveAsPngAsync(coverImageStream, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return TypedResults.Empty;
        }

        ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
        await using Stream keyStream = keyEntry.Open();
        await using StreamWriter keyStreamWriter = new(keyStream);
        await keyStreamWriter.WriteAsync(base64Key);

        return TypedResults.Empty;
    }
}

internal sealed class EncodeBinaryRequest
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
        CancellationToken cancellationToken = context.RequestAborted;
        EncodeBinaryEndpoint endpoint = context.RequestServices.GetRequiredService<EncodeBinaryEndpoint>();
        MultipartReader multipartReader = new(context, endpoint);

        if (endpoint.HasValidationError)
        {
            return null;
        }

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
