﻿using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using steganography.domain;
using ILogger = Serilog.ILogger;

namespace steganography.api.features.codec;

public partial class EncodeBinaryEndpoint : MinimalApiBuilderEndpoint
{
    private static async Task HandleAsync(
        EncodeBinaryRequest request,
        [FromServices] EncodeBinaryEndpoint endpoint,
        [FromServices] ILogger logger,
        [FromServices] IKeyService keyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        logger.Information(
            "Encoding binary message with cover image (width: {Width}, height: {Height})",
            request.CoverImage.Width, request.CoverImage.Height);

        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        int? messageLength;

        using AesCounterMode aes = new();
        using Encoder encoder = new(request.CoverImage, seed);

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes, endpoint.ValidationErrors, cancellationToken);
            Task reading = encoder.EncodeAsync(request.PipeReader, cancellationToken);
            await Task.WhenAll(writing, reading);
            messageLength = writing.Result;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (InvalidOperationException e)
        {
            endpoint.AddValidationError(e.Message);
            await endpoint.SendErrorAsync(httpContext, "Encoding failed", cancellationToken: cancellationToken);
            return;
        }

        if (!messageLength.HasValue)
        {
            await endpoint.SendErrorAsync(httpContext, "Encoding failed", cancellationToken: cancellationToken);
            return;
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
            return;
        }

        ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
        await using Stream keyStream = keyEntry.Open();
        await using StreamWriter keyStreamWriter = new(keyStream);
        await keyStreamWriter.WriteAsync(base64Key);
    }
}
