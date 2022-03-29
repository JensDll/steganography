using System.IO.Compression;
using System.Security.Cryptography;
using ApiBuilder;
using Domain.Entities;
using Domain.Enums;
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
        _logger.Information("Encoding text message with cover image (width: {Width}, height: {Height})",
            request.CoverImage.Width, request.CoverImage.Height);

        int seed = RandomNumberGenerator.GetInt32(int.MaxValue);
        using AesCounterMode aes = new();
        using Encoder encoder = new(request.CoverImage, seed, request.CancelSource);

        int? messageLength;

        try
        {
            Task<int?> writing = request.FillPipeAsync(aes);
            Task reading = encoder.EncodeAsync(request.PipeReader);
            await Task.WhenAll(writing, reading);
            messageLength = writing.Result;
        }
        catch (InvalidOperationException e)
        {
            ValidationErrors.Add(e.Message);
            await SendValidationErrorAsync("Encoding failed");
            return;
        }
        catch (OperationCanceledException)
        {
            await SendValidationErrorAsync("Encoding failed");
            return;
        }

        if (!messageLength.HasValue)
        {
            await SendValidationErrorAsync("Encoding failed");
            return;
        }

        string base64Key = _keyService.ToBase64(MessageType.Text, seed, messageLength.Value, aes.Key, aes.IV);

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
}
