using System.IO.Compression;
using ApiBuilder;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using SixLabors.ImageSharp;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.EncodeText;

public class EncodeText : EndpointWithoutResponse<Request>
{
    private readonly IEncodeService _encodeService;
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;
    private readonly IDataProtectionProvider _protectionProvider;

    public EncodeText(IEncodeService encodeService, IKeyService keyService, IDataProtectionProvider protectionProvider,
        ILogger logger)
    {
        _encodeService = encodeService;
        _keyService = keyService;
        _protectionProvider = protectionProvider;
        _logger = logger;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            ushort seed = (ushort) Random.Shared.Next();
            string key = _keyService.Generate(128);
            IDataProtector protector = _protectionProvider.CreateProtector(key);
            byte[] protectedMessage = protector.Protect(request.Message);
            key = _keyService.AddMetaData(key, seed, protectedMessage.Length);

            _encodeService.Encode(request.CoverImage, protectedMessage, seed);

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);
            ZipArchiveEntry coverImageEntry = archive.CreateEntry("image.png", CompressionLevel.Fastest);
            await using (Stream coverImageStream = coverImageEntry.Open())
            {
                await request.CoverImage.SaveAsPngAsync(coverImageStream, cancellationToken);
            }

            ZipArchiveEntry keyEntry = archive.CreateEntry("key.txt", CompressionLevel.Fastest);
            await using Stream keyStream = keyEntry.Open();
            await using StreamWriter writer = new(keyStream);
            await writer.WriteAsync(key);
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
