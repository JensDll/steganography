using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using steganography.domain;
using ILogger = Serilog.ILogger;

namespace steganography.api.features.codec;

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
        logger.Information("Encoding text message with cover image (width: {Width}, height: {Height})",
            request.CoverImage.Width, request.CoverImage.Height);

        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        int? messageLength;

        using AesCounterMode aes = new();
        using Encoder encoder = new(request.CoverImage, seed);

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes, endpoint, cancellationToken);
            Task reading = encoder.EncodeAsync(request.PipeReader, cancellationToken);
            await Task.WhenAll(writing, reading);
            messageLength = writing.Result;
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
