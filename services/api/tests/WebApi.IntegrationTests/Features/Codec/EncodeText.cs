using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.IntegrationTests.Features.Codec;

public class EncodeText : TestingBase
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

    [Test]
    public async Task BadRequestWhenTheMessageIsTooLong()
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
    public async Task BadRequestForInvalidFormData()
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
}
