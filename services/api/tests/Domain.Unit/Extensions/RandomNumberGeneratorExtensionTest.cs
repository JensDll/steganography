using System.Security.Cryptography;
using Domain.Extensions;
using NUnit.Framework;

namespace Domain.Unit.Extensions;

[TestFixture]
internal class RandomNumberGeneratorExtensionTest
{
    [TestCase(4, 4)]
    [TestCase(8, 8)]
    [TestCase(16, 16)]
    [TestCase(32, 32)]
    [TestCase(64, 64)]
    [TestCase(128, 128)]
    [TestCase(256, 256)]
    [TestCase(512, 512)]
    // Should round to multiple of four
    [TestCase(6, 4)]
    [TestCase(13, 12)]
    [TestCase(21, 20)]
    public void ShouldGenerateKeyOfGivenLength(int length, int expectedLength)
    {
        // Arrange
        RandomNumberGenerator rng = RandomNumberGenerator.Create();

        // Act
        string key = rng.GenerateKey(length);

        // Assert
        Assert.That(key, Has.Length.EqualTo(expectedLength));
    }
}
