using System;
using Domain.Services;
using Moq;
using NUnit.Framework;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.IntegrationTests;

[TestFixture]
internal class EncodeDecodeTests
{
    [TestCase(10)]
    [TestCase(100_000)]
    [TestCase(400_000)]
    [TestCase(700_000)]
    [TestCase(740_000)]
    public void EncodeDecode_MessageShouldBeTheSame(int messageLength)
    {
        // Arrange
        Mock<ILogger> loggerMock = new();

        EncodeService encodeService = new(loggerMock.Object);
        DecodeService decodeService = new(loggerMock.Object);

        byte[] message = new byte[messageLength];
        Random.Shared.NextBytes(message);

        Image<Rgb24> coverImage = new(500, 500);

        // Act
        encodeService.Encode(coverImage, message, 10);
        byte[] result = decodeService.Decode(coverImage, 10, messageLength);

        // Assert
        Assert.That(message, Is.EqualTo(result));
    }
}
