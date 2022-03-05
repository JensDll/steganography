using System.IO.Compression;
using ApiBuilder;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace WebApi.Features.Codec.Decode;

public class Decode : EndpointWithoutResponse<Request>
{
    private readonly IDecoder _decoder;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IDataProtectionProvider _protectionProvider;

    public Decode(IDecoder decoder, IDataProtectionProvider protectionProvider, IKeyGenerator keyGenerator)
    {
        _decoder = decoder;
        _protectionProvider = protectionProvider;
        _keyGenerator = keyGenerator;
    }

    protected override async Task HandleAsync(Request request)
    {
        if (!_keyGenerator.TryParseKey(request.Key, out ushort seed, out int messageLength, out string key))
        {
            await SendValidationErrorAsync("Decoding failed");
            return;
        }

        IDataProtector protector = _protectionProvider.CreateProtector(key);
        byte[] message;

        try
        {
            message = _decoder.Decode(request.CoverImage, seed, messageLength);
            message = protector.Unprotect(message);
        }
        catch (Exception e)
        {
            await SendValidationErrorAsync("Decoding failed");
            return;
        }

        List<DecodedItem> items = _decoder.ParseMessage(message, out bool isText);

        if (isText)
        {
            await SendTextAsync(items[0].Data);
            return;
        }

        HttpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

        foreach (DecodedItem item in items)
        {
            ZipArchiveEntry entry = archive.CreateEntry(item.Name, CompressionLevel.Fastest);
            await using Stream entryStream = entry.Open();
            await entryStream.WriteAsync(item.Data);
        }
    }
}
