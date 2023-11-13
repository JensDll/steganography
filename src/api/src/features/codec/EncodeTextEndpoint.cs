using System.IO.Compression;
using System.IO.Pipelines;
using System.Security.Cryptography;
using api.extensions;
using domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ILogger = Serilog.ILogger;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace api.features.codec;

public partial class EncodeTextEndpoint : MinimalApiBuilderEndpoint
{
    private static async Task<Results<EmptyHttpResult, ValidationProblem>> HandleAsync(
        EncodeTextRequest request,
        [FromServices] EncodeTextEndpoint endpoint,
        [FromServices] ILogger logger,
        [FromServices] IKeyService keyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        logger.Information("Encoding text message with cover image width {Width} height {Height}",
            request.CoverImage.Width, request.CoverImage.Height);

        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        int? messageLength;

        using AesCounterMode aes = new();
        using ImageEncoder imageEncoder = new(request.CoverImage, seed);

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes, endpoint, cancellationToken);
            Task reading = imageEncoder.EncodeAsync(request.PipeReader, cancellationToken);
            await Task.WhenAll(writing, reading);
#pragma warning disable CA1849 // The task is guaranteed to be completed at this point.
            messageLength = writing.Result;
#pragma warning restore CA1849
        }
        catch (OperationCanceledException)
        {
            return TypedResults.Empty;
        }
        catch (InvalidOperationException e)
        {
            endpoint.AddValidationError(e.Message);
            return TypedResults.ValidationProblem(endpoint.ValidationErrors, title: "Encoding failed.");
        }

        if (!messageLength.HasValue)
        {
            return TypedResults.ValidationProblem(endpoint.ValidationErrors, title: "Encoding failed.");
        }

        string base64Key = keyService.ToBase64String(MessageType.Text, seed, messageLength.Value, aes.Key,
            aes.InitializationValue);

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
