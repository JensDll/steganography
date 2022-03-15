using System;
using Domain.Services;
using Moq;
using NUnit.Framework;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.UnitTests.Services;

[TestFixture]
internal class EncodeServiceTests
{
    [Test]
    public void Encode_ShouldNotThrow_WhenTheMessageIsTheMaximumSize()
    {
        // Arrange
        Mock<ILogger> loggerMock = new();

        EncodeService encodeService = new(loggerMock.Object);
        byte[] message = new byte[750_000];
        Random.Shared.NextBytes(message);
        Image<Rgb24> coverImage = new(500, 500);

        // Act
        void Action()
        {
            encodeService.Encode(coverImage, message, 10);
        }

        // Assert
        Assert.That(Action, Throws.Nothing);
    }

    [Test]
    public void Encode_ShouldThrowOutOfRangeException_WhenTheMessageIsTooLong()
    {
        // Arrange
        Mock<ILogger> loggerMock = new();

        EncodeService encodeService = new(loggerMock.Object);
        byte[] message = new byte[750_001];
        Random.Shared.NextBytes(message);
        Image<Rgb24> coverImage = new(500, 500);

        // Act
        void Action()
        {
            encodeService.Encode(coverImage, message, 10);
        }

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
