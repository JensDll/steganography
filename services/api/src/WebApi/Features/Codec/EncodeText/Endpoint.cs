using System.IO.Compression;
using ApiBuilder;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using SixLabors.ImageSharp;

namespace WebApi.Features.Codec.EncodeText;

public class EncodeText : EndpointWithoutResponse<Request>
{
    private readonly IEncodeService _encodeService;
    private readonly IKeyService _keyService;
    private readonly IDataProtectionProvider _protectionProvider;

    public EncodeText(IEncodeService encodeService, IKeyService keyService, IDataProtectionProvider protectionProvider)
    {
        _encodeService = encodeService;
        _keyService = keyService;
        _protectionProvider = protectionProvider;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            // Protect and encode the message
            ushort seed = (ushort) Random.Shared.Next();
            string base64Key = _keyService.GenerateKey();

            IDataProtector protector = _protectionProvider.CreateProtector(base64Key);
            byte[] protectedMessage = protector.Protect(request.Message);

            base64Key = _keyService.AddMetaData(base64Key, messageType: MessageType.Text, seed: seed,
                messageLength: protectedMessage.Length);

            _encodeService.Encode(request.CoverImage, protectedMessage, seed);

            // Write the response
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
            await writer.WriteAsync(base64Key);
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
