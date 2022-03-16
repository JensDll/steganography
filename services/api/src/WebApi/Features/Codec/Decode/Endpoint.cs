using System.IO.Compression;
using ApiBuilder;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.Decode;

public class Decode : EndpointWithoutResponse<Request>
{
    private readonly IDecodeService _decodeService;
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;
    private readonly IDataProtectionProvider _protectionProvider;

    public Decode(IDecodeService decodeService, IDataProtectionProvider protectionProvider, IKeyService keyService,
        ILogger logger)
    {
        _decodeService = decodeService;
        _protectionProvider = protectionProvider;
        _keyService = keyService;
        _logger = logger;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_keyService.TryParse(request.Key, out ushort seed, out int messageLength, out string key))
            {
                await SendValidationErrorAsync("Decoding failed");
                return;
            }

            IDataProtector protector = _protectionProvider.CreateProtector(key);
            byte[] message;

            try
            {
                message = _decodeService.Decode(request.CoverImage, seed, messageLength);
                message = protector.Unprotect(message);
            }
            catch
            {
                await SendValidationErrorAsync("Decoding failed");
                return;
            }

            List<DecodedItem> items = _decodeService.ParseMessage(message, out bool isText);

            if (isText)
            {
                await SendTextAsync(items[0].Data);
                return;
            }

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

            foreach (DecodedItem item in items)
            {
                ZipArchiveEntry entry = archive.CreateEntry(item.Name, CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                await entryStream.WriteAsync(item.Data, cancellationToken);
            }
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
