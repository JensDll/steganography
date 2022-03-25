using System.IO.Compression;
using System.IO.Pipelines;
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
        if (!_keyService.TryParse(request.Key, out MessageType messageType, out int seed,
                out int messageLength, out byte[] key, out byte[] iV))
        {
            ValidationErrors.Add("Invalid key");
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

            bool hasNextFile = decoder.TryDecodeNextFileInfo(out string fileName, out int fileLength);

            if (hasNextFile)
            {
                using ZipArchive archive = new(HttpContext.Response.BodyWriter.AsStream(), ZipArchiveMode.Create);

                do
                {
                    ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                    await using Stream entryStream = entry.Open();
                    PipeWriter entryStreamWriter = PipeWriter.Create(entryStream);
                    await decoder.DecodeAsync(entryStreamWriter, fileLength);
                } while (decoder.TryDecodeNextFileInfo(out fileName, out fileLength));
            }
        }
        catch (InvalidOperationException e)
        {
            if (!HttpContext.Response.HasStarted)
            {
                ValidationErrors.Add(e.Message);
                await SendValidationErrorAsync("Decoding failed");
            }
        }
    }
}
