using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using Domain;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Steganography.Test.Features.Codec;

internal sealed class EncodeBinaryTests
{
    private static readonly Uri s_encodeUri = new("/api/v1/codec/encode/binary", UriKind.Relative);
    private static readonly Uri s_decodeUri = new("/api/v1/codec/decode", UriKind.Relative);

    [Test]
    public async Task Encode_Decode([Values] bool isSameKey)
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent encodeFormData = [];

        using StreamContent coverImageContent = new(coverImageStream);
        encodeFormData.Add(coverImageContent, "coverImage", "coverImage.png");

        byte[] file1Data = new byte[10];
        Random.Shared.NextBytes(file1Data);
        File file1 = new()
        {
            Name = "file1.txt",
            Content = file1Data,
            Size = file1Data.Length
        };
        using StringContent file1LengthContent = new(file1.Size.ToString(CultureInfo.InvariantCulture));
        encodeFormData.Add(file1LengthContent, "length");
        using ByteArrayContent file1Content = new(file1.Content);
        encodeFormData.Add(file1Content, "file1", file1.Name);

        byte[] file2Data = new byte[10_000];
        Random.Shared.NextBytes(file2Data);
        File file2 = new()
        {
            Name = "file2.txt",
            Content = file2Data,
            Size = file2Data.Length
        };
        using StringContent file2LengthContent = new(file2.Size.ToString(CultureInfo.InvariantCulture));
        encodeFormData.Add(file2LengthContent, "length");
        using ByteArrayContent file2Content = new(file2.Content);
        encodeFormData.Add(file2Content, "file2", file2.Name);

        byte[] file3Data = new byte[120_000];
        Random.Shared.NextBytes(file3Data);
        File file3 = new()
        {
            Name = "file3.txt",
            Content = file3Data,
            Size = file3Data.Length
        };
        using StringContent file3LengthContent = new(file3.Size.ToString(CultureInfo.InvariantCulture));
        encodeFormData.Add(file3LengthContent, "length");
        using ByteArrayContent file3Content = new(file3.Content);
        encodeFormData.Add(file3Content, "file3", file3.Name);

        File[] files = { file1, file2, file3 };

        int inMessageLength = GetMessageLength(files);

        // Act
        HttpResponseMessage encodeResponse = await TestSetup.Client.PostAsync(s_encodeUri, encodeFormData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));

        await using Stream encodeResponseStream = await encodeResponse.Content.ReadAsStreamAsync();
        using ZipArchive encodeArchive = new(encodeResponseStream, ZipArchiveMode.Read);

        Assert.That(encodeArchive.Entries, Has.Count.EqualTo(2));
        Assert.That(encodeArchive.Entries[0].Name, Is.EqualTo("image.png"));
        Assert.That(encodeArchive.Entries[1].Name, Is.EqualTo("key.txt"));

        await using Stream zipImageStream = encodeArchive.Entries[0].Open();
        Image<Rgb24> resultImage = await Image.LoadAsync<Rgb24>(zipImageStream);

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

        using MultipartFormDataContent decodeFormData = [];

        using StreamContent resultImageContent = new(resultImageStream);
        decodeFormData.Add(resultImageContent, "coverImage", "coverImage.png");

        using StringContent keyContent = new(key);
        decodeFormData.Add(keyContent, "key");

        // Act
        HttpResponseMessage decodeResponse = await TestSetup.Client.PostAsync(s_decodeUri, decodeFormData);

        // Assert
        if (isSameKey)
        {
            Assert.That(decodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(decodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/zip"));
        }
        else
        {
            return;
        }

        await using Stream decodeResponseStream = await decodeResponse.Content.ReadAsStreamAsync();
        using ZipArchive decodeArchive = new(decodeResponseStream, ZipArchiveMode.Read);

        Assert.That(decodeArchive.Entries, Has.Count.EqualTo(files.Length));

        for (int i = 0; i < files.Length; ++i)
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
    public async Task BadRequest_When_The_Message_Is_Too_Long()
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent formData = [];

        using StreamContent coverImageContent = new(coverImageStream);
        formData.Add(coverImageContent, "coverImage", "coverImage.png");

        using ByteArrayContent fileContent = new(new byte[750_000]);
        formData.Add(fileContent, "name", "file.png");

        // Act
        HttpResponseMessage encodeResponse = await TestSetup.Client.PostAsync(s_encodeUri, formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
    }

    [Test]
    public async Task BadRequest_For_Invalid_Form_Data()
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent formData = [];

        using StreamContent coverImageContent = new(coverImageStream);
        formData.Add(coverImageContent, "coverImage", "coverImage.png");

        using ByteArrayContent fileContent = new(new byte[3]);
        formData.Add(fileContent, "name", "file.png");

        using StringContent invalidContent = new("invalid");
        formData.Add(invalidContent, "name");

        // Act
        HttpResponseMessage encodeResponse = await TestSetup.Client.PostAsync(s_encodeUri, formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
    }

    private static int GetMessageLength(params File[] files)
    {
        // File length + file name length + file name + file
        return files.Sum(file => 4 + 2 + Encoding.UTF8.GetByteCount(file.Name) + file.Content.Length);
    }

    private sealed class File
    {
        public required string Name { get; init; }
        public required byte[] Content { get; init; }
        public required int Size { get; init; }
    }
}
