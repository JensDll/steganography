using System.IO.Compression;
using System.Security.Cryptography;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using SixLabors.ImageSharp;
using ILogger = Serilog.ILogger;

namespace WebApi.Features.Codec.EncodeText;

public class EncodeText : EndpointWithoutResponse<Request>
{
    private readonly IKeyService _keyService;
    private readonly ILogger _logger;

    public EncodeText(IKeyService keyService, ILogger logger)
    {
        _keyService = keyService;
        _logger = logger;
    }

    protected override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Encoding text message ... cover image (width: {Width}, height: {Height})",
                request.CoverImage.Width, request.CoverImage.Height);

            (string base64Key, byte[] key, byte[] iV) = _keyService.GenerateKey();
            ushort seed = (ushort) Random.Shared.Next();

            using Aes aes = Aes.Create();
            ICryptoTransform encryptor = aes.CreateEncryptor(key, iV);

            Encoder encoder = new(request.CoverImage, seed, request.CancelSource);

            try
            {
                Task<bool> writing = request.FillPipeAsync(encryptor);
                Task<int> reading = encoder.EncodeAsync(request.PipeReader);

                await Task.WhenAll(writing, reading);

                if (!writing.Result)
                {
                    await SendValidationErrorAsync("Encoding failed");
                    return;
                }

                base64Key = _keyService.AddMetaData(base64Key, MessageType.Text, seed, reading.Result);
            }
            catch (MessageTooLongException e)
            {
                ValidationErrors.Add(e.Message);
                await SendValidationErrorAsync("Encoding failed");
                return;
            }

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
            await using StreamWriter keyStreamWriter = new(keyStream);
            await keyStreamWriter.WriteAsync(base64Key);
        }
        finally
        {
            request.CoverImage.Dispose();
        }
    }
}
