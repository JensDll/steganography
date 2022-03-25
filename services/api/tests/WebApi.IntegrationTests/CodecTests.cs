using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.IntegrationTests;

[TestFixture]
public class CodecTests : TestingBase
{
    [Test]
    public async Task EncodeDecode_ShouldResultInSameText()
    {
        #region AAA Encode

        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);
        string message = new('a', 750_000);

        MultipartFormDataContent encodeFormData = new();
        encodeFormData.Add(new StreamContent(coverImageStream), "coverImage", "foo.png");
        encodeFormData.Add(new StringContent(message), "message");

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/api/codec/encode/text", encodeFormData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));

        await using Stream encodeResponseStream = await encodeResponse.Content.ReadAsStreamAsync();
        using ZipArchive archive = new(encodeResponseStream, ZipArchiveMode.Read);

        Assert.That(archive.Entries, Has.Count.EqualTo(2));
        Assert.That(archive.Entries[0].Name, Is.EqualTo("image.png"));
        Assert.That(archive.Entries[1].Name, Is.EqualTo("key.txt"));

        await using Stream zipImageStream = archive.Entries[0].Open();
        Image<Rgb24>? resultImage = await Image.LoadAsync<Rgb24>(zipImageStream);

        Assert.That(resultImage, Is.Not.Null);
        Assert.That(resultImage.Width, Is.EqualTo(coverImage.Width));
        Assert.That(resultImage.Height, Is.EqualTo(coverImage.Height));

        using StreamReader reader = new(archive.Entries[1].Open(), Encoding.UTF8);
        string key = await reader.ReadToEndAsync();

        KeyService keyService = new();
        bool success = keyService.TryParse(key, out MessageType outMessageType, out _,
            out int outMessageLength, out _, out _);

        Assert.That(success, Is.True);
        Assert.That(outMessageType, Is.EqualTo(MessageType.Text));
        Assert.That(outMessageLength, Is.EqualTo(message.Length));

        #endregion

        #region AAA Decode

        // Arrange
        await using MemoryStream resultImageStream = new();
        await resultImage.SaveAsPngAsync(resultImageStream);

        MultipartFormDataContent decodeFormData = new();
        decodeFormData.Add(new StreamContent(resultImageStream), "coverImage", "test.png");
        decodeFormData.Add(new StringContent(key), "key");

        // Act
        HttpResponseMessage decodeResponse =
            await Client.PostAsync("/api/codec/decode", decodeFormData);

        // Assert
        Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));

        string resultMessage = await decodeResponse.Content.ReadAsStringAsync();

        Assert.That(resultMessage, Is.EqualTo(message));

        #endregion
    }

    [Test]
    public async Task Encode_ShouldBeBadRequestWhenTheMessageIsTooLong()
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);
        string message = new('a', 750_001);

        MultipartFormDataContent encodeFormData = new();
        encodeFormData.Add(new StreamContent(coverImageStream), "coverImage", "foo.png");
        encodeFormData.Add(new StringContent(message), "message");

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/api/codec/encode/text", encodeFormData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }


    [Test]
    public async Task EncodeDecodeBinary()
    {
        #region AAA Encode

        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent encodeFormData = new();
        encodeFormData.Add(new StreamContent(coverImageStream), "coverImage", "test.png");

        var files = new[] {10, 10_000, 120_000}.Select((size, i) =>
        {
            byte[] content = new byte[size];
            Random.Shared.NextBytes(content);
            return new
            {
                Name = $"file{i + 1}.txt",
                Content = content
            };
        }).ToArray();
        int messageLength = files.Sum(file =>
            // File length + file name length + file name + file
            4 + 2 + Encoding.UTF8.GetByteCount(file.Name) + file.Content.Length);

        foreach (var file in files)
        {
            encodeFormData.Add(new StringContent(file.Content.Length.ToString()));
            encodeFormData.Add(new ByteArrayContent(file.Content), "foo", file.Name);
        }

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/api/codec/encode/binary", encodeFormData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));

        await using Stream encodeResponseStream = await encodeResponse.Content.ReadAsStreamAsync();
        using ZipArchive encodeArchive = new(encodeResponseStream, ZipArchiveMode.Read);

        Assert.That(encodeArchive.Entries, Has.Count.EqualTo(2));
        Assert.That(encodeArchive.Entries[0].Name, Is.EqualTo("image.png"));
        Assert.That(encodeArchive.Entries[1].Name, Is.EqualTo("key.txt"));

        await using Stream zipImageStream = encodeArchive.Entries[0].Open();
        Image<Rgb24>? resultImage = await Image.LoadAsync<Rgb24>(zipImageStream);

        Assert.That(resultImage, Is.Not.Null);
        Assert.That(resultImage.Width, Is.EqualTo(coverImage.Width));
        Assert.That(resultImage.Height, Is.EqualTo(coverImage.Height));

        using StreamReader reader = new(encodeArchive.Entries[1].Open(), Encoding.UTF8);
        string key = await reader.ReadToEndAsync();

        KeyService keyService = new();
        bool success = keyService.TryParse(key, out MessageType outMessageType, out _,
            out int outMessageLength, out _, out _);

        Assert.That(success, Is.True);
        Assert.That(outMessageType, Is.EqualTo(MessageType.Binary));
        Assert.That(outMessageLength, Is.EqualTo(messageLength));

        #endregion

        #region AAA Decode

        // Arrange
        await using MemoryStream resultImageStream = new();
        await resultImage.SaveAsPngAsync(resultImageStream);

        MultipartFormDataContent decodeFormData = new();
        decodeFormData.Add(new StreamContent(resultImageStream), "coverImage", "test.png");
        decodeFormData.Add(new StringContent(key), "key");

        // Act
        HttpResponseMessage decodeResponse =
            await Client.PostAsync("/api/codec/decode", decodeFormData);

        // Assert
        Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));

        await using Stream decodeResponseStream = await decodeResponse.Content.ReadAsStreamAsync();
        using ZipArchive decodeArchive = new(decodeResponseStream, ZipArchiveMode.Read);

        Assert.That(decodeArchive.Entries, Has.Count.EqualTo(files.Length));

        for (int i = 0; i < files.Length; i++)
        {
            ZipArchiveEntry entry = decodeArchive.Entries[i];
            await using MemoryStream fileStream = new();
            await using Stream entryStream = entry.Open();
            await entryStream.CopyToAsync(fileStream);

            Assert.That(entry.Name, Is.EqualTo(files[i].Name));
            Assert.That(fileStream.ToArray(), Is.EqualTo(files[i].Content));
        }

        #endregion
    }
}
