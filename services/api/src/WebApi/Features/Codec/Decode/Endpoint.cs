using System.IO.Compression;
using System.IO.Pipelines;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Net.Http.Headers;
using MinimalApiBuilder;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.Decode;

public partial class DecodeEndpoint : MinimalApiBuilderEndpoint
{
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;

    public DecodeEndpoint(IKeyService keyService, ILogger logger)
    {
        _keyService = keyService;
        _logger = logger;
    }

    public static void Configure(RouteHandlerBuilder builder) { }

    private static async Task HandleAsync(
        DecodeRequest request,
        DecodeEndpoint endpoint,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        bool isValidKey = endpoint._keyService.TryParse(request.Key, out MessageType messageType, out int seed,
            out int messageLength, out byte[] key, out byte[] iV);

        endpoint._logger.Information("Decoding {MessageType} message with valid key {IsValidKey}",
            messageType, isValidKey);

        if (!isValidKey || messageLength < 1 || messageLength > request.CoverImageCapacity)
        {
            await endpoint.SendErrorAsync(httpContext, "Decoding failed", cancellationToken: cancellationToken);
            return;
        }

        using AesCounterMode aes = new(key, iV);
        using Decoder decoder = new(request.CoverImage, seed, messageLength, aes);

        if (messageType == MessageType.Text)
        {
            httpContext.Response.ContentType = "text/plain";

            try
            {
                await decoder.DecodeAsync(httpContext.Response.BodyWriter, cancellationToken);
            }
            catch (OperationCanceledException) { }

            return;
        }

        ContentDispositionHeaderValue contentDisposition = new("attachment");
        contentDisposition.SetHttpFileName("result.zip");
        httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
        httpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(httpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

        while (decoder.DecodeNextFileInformation() is var (fileName, fileLength))
        {
            ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
            await using Stream entryStream = entry.Open();
            PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
            try
            {
                await decoder.DecodeAsync(entryStreamWriter, fileLength, cancellationToken);
            }
            catch (OperationCanceledException) { }
        }
    }
}
