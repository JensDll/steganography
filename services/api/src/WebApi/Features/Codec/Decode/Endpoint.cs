using System.IO.Compression;
using System.IO.Pipelines;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.Decode;

public class Decode : EndpointWithoutResponse<Request>
{
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;

    public Decode(IKeyService keyService, ILogger logger)
    {
        _keyService = keyService;
        _logger = logger;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        bool isValidKey = _keyService.TryParse(request.Key, out MessageType messageType, out int seed,
            out int messageLength, out byte[] key, out byte[] iV);

        _logger.Information(
            "Decoding {MessageType} message with valid key {IsValidKey} and length {MessageLength}",
            messageType, isValidKey, messageLength);

        if (!isValidKey || messageLength < 1 ||
            messageLength > request.CoverImage.Width * request.CoverImage.Height * 3)
        {
            await SendValidationErrorAsync("Decoding failed");
            return;
        }

        using AesCounterMode aes = new(key, iV);
        using Decoder decoder = new(request.CoverImage, seed, messageLength, aes);

        try
        {
            if (messageType == MessageType.Text)
            {
                HttpContext.Response.ContentType = "text/plain";
                await decoder.DecodeAsync(HttpContext.Response.BodyWriter);
                return;
            }

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

            IEnumerable<(string fileName, int fileLength)> fileInformation = decoder.DecodeFileInformation();

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

            foreach ((string fileName, int fileLength) in fileInformation)
            {
                ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
                await decoder.DecodeAsync(entryStreamWriter, fileLength);
            }
        }
        catch (InvalidOperationException e)
        {
            _logger.Information("Decoding failed with message {Message}", e.Message);
            ValidationErrors.Add(e.Message);
            await SendValidationErrorAsync("Decoding failed");
        }
    }
}
