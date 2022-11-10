using System.IO.Compression;
using System.IO.Pipelines;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.Decode;

public class Decode : Endpoint<Request>
{
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;

    public Decode(IKeyService keyService, ILogger logger)
    {
        _keyService = keyService;
        _logger = logger;
    }

    protected override async Task<IResult> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        bool isValidKey = _keyService.TryParse(request.Key, out MessageType messageType, out int seed,
            out int messageLength, out byte[] key, out byte[] iV);

        _logger.Information("Decoding {MessageType} message with valid key {IsValidKey}",
            messageType, isValidKey);

        if (!isValidKey || messageLength < 1 || messageLength > request.CoverImageCapacity)
        {
            return ErrorResult("Decoding failed");
        }

        using AesCounterMode aes = new(key, iV);
        using Decoder decoder = new(request.CoverImage, seed, messageLength, aes);

        try
        {
            if (messageType == MessageType.Text)
            {
                HttpContext.Response.ContentType = "text/plain";
                await decoder.DecodeAsync(HttpContext.Response.BodyWriter, cancellationToken);
                return Results.Empty;
            }

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=result.zip");

            IEnumerable<(string fileName, int fileLength)> fileInformation = decoder.DecodeFileInformation();

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

            foreach ((string fileName, int fileLength) in fileInformation)
            {
                ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
                await decoder.DecodeAsync(entryStreamWriter, fileLength, cancellationToken);
            }
        }
        catch (InvalidOperationException e)
        {
            _logger.Information("Decoding failed: {Message}", e.Message);
            ValidationErrors.Add(e.Message);
            return ErrorResult("Decoding failed");
        }

        return Results.Empty;
    }
}
