using System;
using Domain.Entities;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.IntegrationTests;

[TestFixture]
internal class EncodeDecodeTest
{
    [TestCase(1000)]
    [TestCase(100_000)]
    [TestCase(400_000)]
    [TestCase(700_000)]
    [TestCase(740_000)]
    public void EncodeDecode_MessageShouldBeTheSame(int length)
    {
        // Arrange
        Encoder encoder = new();
        Decoder decoder = new();

        byte[] data = new byte[length];
        new Random().NextBytes(data);

        Image<Rgb24> image = new(500, 500);

        // Act
        encoder.Encode(image, data);
        byte[] result = decoder.Decode(image);

        // Assert
        Assert.That(result, Is.EqualTo(data));
    }
}
