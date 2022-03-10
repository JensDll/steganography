using System.IO.Compression;
using ApiBuilder;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using SixLabors.ImageSharp.Formats.Png;

namespace WebApi.Features.Codec.EncodeText;

public class EncodeText : EndpointWithoutResponse<Request>
{
    private static readonly PngEncoder _pngEncoder = new();

    private readonly IEncoder _encoder;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IDataProtectionProvider _protectionProvider;


    public EncodeText(IEncoder encoder, IKeyGenerator keyGenerator, IDataProtectionProvider protectionProvider)
    {
        _encoder = encoder;
        _keyGenerator = keyGenerator;
        _protectionProvider = protectionProvider;
    }

    protected override async Task HandleAsync(Request request, CancellationToken _)
    {
        ushort seed = (ushort) Random.Shared.Next();
        string key = _keyGenerator.GenerateKey(128);
        IDataProtector protector = _protectionProvider.CreateProtector(key);
        request.Message = protector.Protect(request.Message);
        key = _keyGenerator.AddMetaData(key, seed, request.Message.Length);

        _encoder.Encode(request.CoverImage, request.Message, seed);

        HttpContext.Response.ContentType = "application/zip";
        HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

        using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);
        ZipArchiveEntry coverImageEntry = archive.CreateEntry("image.png", CompressionLevel.Fastest);
        Stream coverImageStream = coverImageEntry.Open();
        await request.CoverImage.SaveAsync(coverImageStream, _pngEncoder);
        request.CoverImage.Dispose();
        await coverImageStream.DisposeAsync();

        ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
        await using Stream keyStream = keyEntry.Open();
        await using StreamWriter writer = new(keyStream);
        await writer.WriteAsync(key);
    }
}
