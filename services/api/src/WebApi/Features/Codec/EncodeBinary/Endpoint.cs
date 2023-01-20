using System.IO.Compression;
using System.Security.Cryptography;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.EncodeBinary;

public partial class EncodeBinaryEndpoint : MinimalApiBuilderEndpoint
{
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;

    public EncodeBinaryEndpoint(IKeyService keyService, ILogger logger)
    {
        _keyService = keyService;
        _logger = logger;
    }

    public static void Configure(RouteHandlerBuilder builder) { }

    private static async Task<IResult> HandleAsync(
        EncodeBinaryRequest request,
        EncodeBinaryEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        endpoint._logger.Information(
            "Encoding binary message with cover image (width: {Width}, height: {Height})",
            request.CoverImage.Width, request.CoverImage.Height);

        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        using AesCounterMode aes = new();
        using Encoder encoder = new(request.CoverImage, seed);

        int? messageLength;

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes, endpoint.AddValidationError, cancellationToken);
            Task reading = encoder.EncodeAsync(request.PipeReader, cancellationToken);
            await Task.WhenAll(writing, reading);
            messageLength = writing.Result;
        }
        catch (InvalidOperationException e)
        {
            endpoint.AddValidationError(e.Message);
            return endpoint.ErrorResult("Encoding failed");
        }

        if (!messageLength.HasValue)
        {
            return endpoint.ErrorResult("Encoding failed");
        }

        string base64Key = endpoint._keyService.ToBase64String(MessageType.Binary, seed,
            messageLength.Value, aes.Key, aes.IV);

        return Results.Extensions.BodyWriterStream(async stream =>
            {
                using ZipArchive archive = new(stream, ZipArchiveMode.Create);

                ZipArchiveEntry coverImageEntry = archive.CreateEntry("image.png", CompressionLevel.Fastest);
                await using (Stream coverImageStream = coverImageEntry.Open())
                {
                    await request.CoverImage.SaveAsPngAsync(coverImageStream);
                }

                ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
                await using Stream keyStream = keyEntry.Open();
                await using StreamWriter keyStreamWriter = new(keyStream);
                await keyStreamWriter.WriteAsync(base64Key);
            },
            "application/zip", "secret.zip");
    }
}
