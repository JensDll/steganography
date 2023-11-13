using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using steganography.domain;

namespace steganography.api.tests.features.codec;

public class EncodeBinaryTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Encode_Decode(bool isSameKey)
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent encodeFormData = new();

        using StreamContent coverImageContent = new(coverImageStream);

        byte[] file1Content = new byte[10];
        Random.Shared.NextBytes(file1Content);
        File file1 = new()
        {
            Name = "file1.txt",
            Content = file1Content,
            Size = file1Content.Length
        };
        using StringContent file1LengthContent = new(file1.Size.ToString(CultureInfo.InvariantCulture));
        using ByteArrayContent file1ContentContent = new(file1.Content);

        byte[] file2Content = new byte[10_000];
        Random.Shared.NextBytes(file2Content);
        File file2 = new()
        {
            Name = "file2.txt",
            Content = file2Content,
            Size = file2Content.Length
        };
        using StringContent file2LengthContent = new(file2.Size.ToString(CultureInfo.InvariantCulture));
        using ByteArrayContent file2ContentContent = new(file2.Content);

        byte[] file3Content = new byte[120_000];
        Random.Shared.NextBytes(file3Content);
        File file3 = new()
        {
            Name = "file3.txt",
            Content = file3Content,
            Size = file3Content.Length
        };
        using StringContent file3LengthContent = new(file3.Size.ToString(CultureInfo.InvariantCulture));
        using ByteArrayContent file3ContentContent = new(file3.Content);

        encodeFormData.Add(coverImageContent, "coverImage", "coverImage.png");
        encodeFormData.Add(file1LengthContent, "length");
        encodeFormData.Add(file1ContentContent, "name", file1.Name);
        encodeFormData.Add(file2LengthContent, "length");
        encodeFormData.Add(file2ContentContent, "name", file2.Name);
        encodeFormData.Add(file3LengthContent, "length");
        encodeFormData.Add(file3ContentContent, "name", file3.Name);

        int inMessageLength = GetMessageLength(file1, file2, file3);

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync(new Uri("/api/codec/encode/binary"), encodeFormData);

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

        using MultipartFormDataContent decodeFormData = new();

        using StreamContent resultImageContent = new(resultImageStream);
        using StringContent keyContent = new(key);

        decodeFormData.Add(resultImageContent, "coverImage", "coverImage.png");
        decodeFormData.Add(keyContent, "key");

        // Act
        HttpResponseMessage decodeResponse =
            await TestSetup.Client.PostAsync(new Uri("/api/codec/decode"), decodeFormData);

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

        Assert.That(decodeArchive.Entries, Has.Count.EqualTo(3));

        // for (int i = 0; i < files.Length; i++)
        // {
        //     ZipArchiveEntry entry = decodeArchive.Entries[i];
        //     await using MemoryStream fileStream = new();
        //     await using Stream entryStream = entry.Open();
        //     await entryStream.CopyToAsync(fileStream);
        //
        //     Assert.That(entry.Name, Is.EqualTo(files[i].Name));
        //     Assert.That(fileStream.ToArray(), Is.EqualTo(files[i].Content));
        // }
    }

    [Test]
    public async Task BadRequestWhenTheMessageIsTooLong()
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent formData = new();

        using StreamContent coverImageContent = new(coverImageStream);
        using ByteArrayContent fileContent = new(new byte[750_000]);

        formData.Add(coverImageContent, "coverImage", "coverImage.png");
        formData.Add(fileContent, "name", "file.png");

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync(new Uri("/api/codec/encode/binary"), formData);

        // Assert
        Assert.That(encodeResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(encodeResponse.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
    }

    [Test]
    public async Task BadRequest_For_Invalid_FormData()
    {
        // Arrange
        using Image<Rgb24> coverImage = new(500, 500);
        await using MemoryStream coverImageStream = new();
        await coverImage.SaveAsPngAsync(coverImageStream);

        using MultipartFormDataContent formData = new();

        using StreamContent coverImageContent = new(coverImageStream);
        using ByteArrayContent fileContent = new(new byte[3]);
        using StringContent invalidContent = new("invalid");

        formData.Add(coverImageContent, "coverImage", "coverImage.png");
        formData.Add(fileContent, "name", "file.png");
        formData.Add(invalidContent, "name");

        // Act
        HttpResponseMessage encodeResponse =
            await TestSetup.Client.PostAsync(new Uri("/api/codec/encode/binary"), formData);

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
