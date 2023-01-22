using System.IO.Compression;
using System.IO.Pipelines;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
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

    private static async Task<IResult> HandleAsync(
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
            return endpoint.ErrorResult("Decoding failed");
        }

        AesCounterMode aes = new(key, iV);
        httpContext.Response.RegisterForDispose(aes);

        Decoder decoder = new(request.CoverImage, seed, messageLength, aes);
        httpContext.Response.RegisterForDispose(decoder);

        if (messageType == MessageType.Text)
        {
            httpContext.Response.ContentType = "text/plain";

            try
            {
                await decoder.DecodeAsync(httpContext.Response.BodyWriter, cancellationToken);
            }
            catch (OperationCanceledException) { }

            return Results.Empty;
        }

        if (!decoder.TryDecodeFileInformation(out List<(string fileName, int fileLength)> fileInformation))
        {
            return endpoint.ErrorResult("Decoding failed");
        }

        return Results.Extensions.BodyWriterStream(async stream =>
            {
                using ZipArchive archive = new(stream, ZipArchiveMode.Create);

                foreach ((string fileName, int fileLength) in fileInformation)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                    await using Stream entryStream = entry.Open();
                    PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);

                    try
                    {
                        await decoder.DecodeAsync(entryStreamWriter, fileLength, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            },
            "application/zip", "result.zip");
    }
}
