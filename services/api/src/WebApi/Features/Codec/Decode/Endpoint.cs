using System.IO.Compression;
using System.IO.Pipelines;
using System.Security.Cryptography;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace WebApi.Features.Codec.Decode;

public class Decode : EndpointWithoutResponse<Request>
{
    private readonly IKeyService _keyService;

    public Decode(IKeyService keyService)
    {
        _keyService = keyService;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_keyService.TryParse(request.Key, out MessageType messageType, out ushort seed,
                    out int messageLength, out byte[] key, out byte[] iV))
            {
                await SendValidationErrorAsync("Decoding failed");
                return;
            }

            using Aes aes = Aes.Create();
            ICryptoTransform decryptor = aes.CreateDecryptor(key, iV);

            Decoder decoder = new(request.CoverImage, seed, messageLength, decryptor);

            if (messageType == MessageType.Text)
            {
                HttpContext.Response.ContentType = "text/plain";
                await decoder.DecodeAsync(HttpContext.Response.BodyWriter);
                return;
            }

            HttpContext.Response.ContentType = "application/zip";
            HttpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=secret.zip");

            using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

            while (decoder.TryReadNextFileInfo(out string fileName, out int fileLength))
            {
                ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
                await decoder.DecodeAsync(entryStreamWriter, fileLength);
            }
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
