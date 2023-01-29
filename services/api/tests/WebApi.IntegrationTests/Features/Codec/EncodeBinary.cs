using System.IO.Compression;
using System.Net;
using System.Text;
using Domain.Enums;
using Domain.Services;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.IntegrationTests.Features.Codec;

public class EncodeBinary
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task EncodeDecode(bool isSameKey)
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent encodeFormData = new();
        encodeFormData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");

        File[] files = GetFiles(10, 10_000, 120_000);
        int inMessageLength = GetMessageLength(files);

        foreach (File file in files)
        {
            encodeFormData.Add(new ByteArrayContent(file.Content), "name", file.Name);
        }

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync("/codec/encode/binary", encodeFormData);

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

        if (!isSameKey)
        {
            key = TestHelper.AlterBase64Key(key);
        }

        KeyService keyService = new();
        bool isValidKey = keyService.TryParse(key, out MessageType outMessageType, out _,
            out int outMessageLength, out _, out _);

        Assert.That(isValidKey, Is.True);
        Assert.That(outMessageType, Is.EqualTo(MessageType.Binary));
        Assert.That(outMessageLength, Is.EqualTo(inMessageLength));

        // Arrange
        await using MemoryStream resultImageStream = new();
        await resultImage.SaveAsPngAsync(resultImageStream);

        MultipartFormDataContent decodeFormData = new();
        decodeFormData.Add(new StreamContent(resultImageStream), "coverImage", "coverImage.png");
        decodeFormData.Add(new StringContent(key), "key");

        // Act
        HttpResponseMessage decodeResponse =
            await TestSetup.Client.PostAsync("/codec/decode", decodeFormData);

        // Assert
        if (isSameKey)
        {
            Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));
        }
        else
        {
            Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            return;
        }

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
    }

    [Test]
    public async Task BadRequestWhenTheMessageIsTooLong()
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent formData = new();
        formData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");
        formData.Add(new ByteArrayContent(new byte[750_000]), "name", "file.png");

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync("/codec/encode/binary", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task BadRequestForInvalidFormData()
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent formData = new();
        formData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");
        formData.Add(new ByteArrayContent(new byte[3]), "name", "file.png");
        formData.Add(new StringContent("invalid"), "name");

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync("/codec/encode/binary", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    private static File[] GetFiles(params int[] sizes) => sizes.Select((size, i) =>
    {
        byte[] content = new byte[size];
        Random.Shared.NextBytes(content);

        File file = new()
        {
            Name = $"file@{i + 1}.txt",
            Content = content
        };

        return file;
    }).ToArray();

    private static int GetMessageLength(IEnumerable<File> files) =>
        // File length + file name length + file name + file
        files.Sum(file => 4 + 2 + Encoding.UTF8.GetByteCount(file.Name) + file.Content.Length);

    private class File
    {
        public required string Name { get; init; }
        public required byte[] Content { get; init; }
    }
}
