using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.IntegrationTests.Features;

[TestFixture]
public class CodecTests : TestingBase
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task EncodeText_Decode(bool isSameKey)
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent encodeFormData = new();
        encodeFormData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");

        string message = new('a', 750_000);
        encodeFormData.Add(new StringContent(message), "message");

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/codec/encode/text", encodeFormData);

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

        if (!isSameKey)
        {
            key = AlterBase64Key(key);
        }

        KeyService keyService = new();
        bool success = keyService.TryParse(key, out MessageType outMessageType, out _,
            out int outMessageLength, out _, out _);

        Assert.That(success, Is.True);
        Assert.That(outMessageType, Is.EqualTo(MessageType.Text));
        Assert.That(outMessageLength, Is.EqualTo(message.Length));

        // Arrange
        await using MemoryStream resultImageStream = new();
        await resultImage.SaveAsPngAsync(resultImageStream);

        MultipartFormDataContent decodeFormData = new();
        decodeFormData.Add(new StreamContent(resultImageStream), "coverImage", "coverImage.png");
        decodeFormData.Add(new StringContent(key), "key");

        // Act
        HttpResponseMessage decodeResponse =
            await Client.PostAsync("/codec/decode", decodeFormData);

        // Assert
        Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/plain"));

        string resultMessage = await decodeResponse.Content.ReadAsStringAsync();

        Assert.That(resultMessage, isSameKey ? Is.EqualTo(message) : Is.Not.EqualTo(message));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task EncodeBinary_Decode(bool isSameKey)
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
            await Client.PostAsync("/codec/encode/binary", encodeFormData);

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
            key = AlterBase64Key(key);
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
            await Client.PostAsync("/codec/decode", decodeFormData);

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
    public async Task EncodeText_BadRequestWhenTheMessageIsTooLong()
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent formData = new();
        formData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");
        formData.Add(new StringContent(new string('a', 750_001)), "message");

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/codec/encode/text", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task EncodeText_BadRequestForInvalidFormData()
    {
        // Arrange
        Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        MultipartFormDataContent formData = new();
        formData.Add(new StreamContent(coverImageStream), "coverImage", "coverImage.png");
        formData.Add(new StringContent(new string('a', 10)), "invalid");

        // Act
        HttpResponseMessage encodeResponse =
            await Client.PostAsync("/codec/encode/text", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task EncodeBinary_BadRequestWhenTheMessageIsTooLong()
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
            await Client.PostAsync("/codec/encode/binary", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    [Test]
    public async Task EncodeBinary_BadRequestForInvalidFormData()
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
            await Client.PostAsync("/codec/encode/binary", formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
    }

    private static string AlterBase64Key(string key)
    {
        StringBuilder keyBuilder = new(key);

        int i = _base64Chars.FindIndex(c => c == keyBuilder[^1]);

        if (i == _base64Chars.Count)
        {
            keyBuilder[^1] = _base64Chars[0];
        }
        else
        {
            keyBuilder[^1] = _base64Chars[i + 1];
        }

        return keyBuilder.ToString();
    }

    private static File[] GetFiles(params int[] sizes)
    {
        return sizes.Select((size, i) =>
        {
            byte[] content = new byte[size];
            Random.Shared.NextBytes(content);
            File file = new($"file@{i + 1}.txt", content);
            return file;
        }).ToArray();
    }

    private static int GetMessageLength(IEnumerable<File> files)
    {
        return files.Sum(file =>
            // File length + file name length + file name + file
            4 + 2 + Encoding.UTF8.GetByteCount(file.Name) + file.Content.Length);
    }

    private class File
    {
        public File(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public string Name { get; }

        public byte[] Content { get; }
    }

    private static readonly List<char> _base64Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray().ToList();
}
