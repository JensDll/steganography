using System.IO.Compression;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using MiniApi;
using WebApi.Endpoints.Codec.Request;

namespace WebApi.Endpoints.Codec.Endpoint;

public class DecodeEndpoint : Endpoint<DecodeRequest>
{
    private readonly IDecoder _decoder;
    private readonly IDataProtectionProvider _protectionProvider;

    public DecodeEndpoint(IDecoder decoder, IDataProtectionProvider protectionProvider)
    {
        _decoder = decoder;
        _protectionProvider = protectionProvider;
    }

    public override async Task HandleAsync(DecodeRequest request)
    {
        IDataProtector protector = _protectionProvider.CreateProtector(request.Key);
        byte[] data = _decoder.Decode(request.CoverImage);
        data = protector.Unprotect(data);

        List<DecodedItem> items = _decoder.ParseBytes(data, out bool isText);

        if (isText)
        {
            HttpContext.Response.ContentType = "text/plain";
            await HttpContext.Response.Body.WriteAsync(items[0].Data);
            return;
        }

        HttpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(HttpContext.Response.Body, ZipArchiveMode.Create, true);

        foreach (DecodedItem item in items)
        {
            ZipArchiveEntry entry = archive.CreateEntry(item.Name, CompressionLevel.Fastest);
            await using Stream entryStream = entry.Open();
            await entryStream.WriteAsync(item.Data);
        }
    }
}
