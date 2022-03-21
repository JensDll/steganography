using System.IO.Compression;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Primitives;

namespace WebApi.Features.Codec.Decode;

public class Decode : EndpointWithoutResponse<Request>
{
    private readonly IDecodeService _decodeService;
    private readonly IKeyService _keyService;
    private readonly IDataProtectionProvider _protectionProvider;

    public Decode(IDecodeService decodeService, IDataProtectionProvider protectionProvider, IKeyService keyService)
    {
        _decodeService = decodeService;
        _protectionProvider = protectionProvider;
        _keyService = keyService;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            // Decode and unprotect the message
            if (!_keyService.TryParse(request.Key, out MessageType messageType, out ushort seed,
                    out int messageLength,
                    out StringSegment key))
            {
                await SendValidationErrorAsync("Decoding failed");
                return;
            }

            IDataProtector protector = _protectionProvider.CreateProtector(key.Value);

            byte[] message = _decodeService.Decode(request.CoverImage, seed, messageLength);
            message = protector.Unprotect(message);

            // Write the response
            if (messageType == MessageType.Text)
            {
                await SendTextAsync(message);
                return;
            }

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

            foreach (DecodedFile file in _decodeService.ParseFiles(message))
            {
                ZipArchiveEntry entry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                await entryStream.WriteAsync(file.Data, cancellationToken);
            }
        }
        catch
        {
            await SendValidationErrorAsync("Decoding failed");
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
