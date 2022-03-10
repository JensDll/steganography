using System;
using Domain.Entities;
using NUnit.Framework;
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
        Encoder encoder = new();
        Decoder decoder = new();

        byte[] message = new byte[messageLength];
        Random.Shared.NextBytes(message);

        Image<Rgb24> coverImage = new(500, 500);

        // Act
        encoder.Encode(coverImage, message, 10);
        byte[] result = decoder.Decode(coverImage, 10, messageLength);

        // Assert
        Assert.That(message, Is.EqualTo(result));
    }
}
