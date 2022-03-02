using System.IO.Compression;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using MiniApi;
using SixLabors.ImageSharp.Formats.Png;
using WebApi.Endpoints.Codec.Request;

namespace WebApi.Endpoints.Codec.Endpoint;

public class EncodeBinaryEndpoint : Endpoint<EncodeBinaryRequest>
{
    private readonly IEncoder _encoder;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IDataProtectionProvider _protectionProvider;

    public EncodeBinaryEndpoint(IEncoder encoder, IKeyGenerator keyGenerator,
        IDataProtectionProvider protectionProvider)
    {
        _encoder = encoder;
        _keyGenerator = keyGenerator;
        _protectionProvider = protectionProvider;
    }

    public override async Task HandleAsync(EncodeBinaryRequest request)
    {
        string key = _keyGenerator.GenerateKey(128);
        IDataProtector protector = _protectionProvider.CreateProtector(key);
        request.Data = protector.Protect(request.Data);

        _encoder.Encode(request.CoverImage, request.Data);

        HttpContext.Response.ContentType = "application/zip";

        using ZipArchive archive = new(HttpContext.Response.Body, ZipArchiveMode.Create, true);
        ZipArchiveEntry coverImageEntry = archive.CreateEntry("image.png", CompressionLevel.Fastest);
        await using Stream coverImageStream = coverImageEntry.Open();
        await request.CoverImage.SaveAsync(coverImageStream, new PngEncoder());
        request.CoverImage.Dispose();

        ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
        await using Stream keyStream = keyEntry.Open();
        await using StreamWriter writer = new(keyStream);
        await writer.WriteAsync(key);
    }
}
