using System;
using Domain.Entities;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.UnitTests.Entities;

[TestFixture]
public class EncoderTest
{
    [Test]
    public void ShouldThrowOutOfRangeExceptionForTooLongData()
    {
        // Arrange
        Encoder encoder = new();

        byte[] data = new byte[750_000];
        new Random().NextBytes(data);

        Image<Rgb24> image = new(500, 500);

        // Act and Assert
        Assert.That(() => { encoder.Encode(image, data); }, Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}
