using System;
using Domain.Entities;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.UnitTests.Entities;

[TestFixture]
internal class EncoderTests
{
    [Test]
    public void Encode_ShouldNotThrow_WhenTheMessageIsTheMaximumSize()
    {
        // Arrange
        Encoder encoder = new();
        byte[] message = new byte[750_000];
        Random.Shared.NextBytes(message);
        Image<Rgb24> coverImage = new(500, 500);

        // Act
        void Action()
        {
            encoder.Encode(coverImage, message, 10);
        }

        // Assert
        Assert.That(Action, Throws.Nothing);
    }

    [Test]
    public void Encode_ShouldThrowOutOfRangeException_WhenTheMessageIsTooLong()
    {
        // Arrange
        Encoder encoder = new();
        byte[] message = new byte[750_001];
        Random.Shared.NextBytes(message);
        Image<Rgb24> coverImage = new(500, 500);

        // Act
        void Action()
        {
            encoder.Encode(coverImage, message, 10);
        }

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
