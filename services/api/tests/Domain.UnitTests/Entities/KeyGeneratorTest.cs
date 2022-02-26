using Domain.Entities;
using NUnit.Framework;

namespace Domain.UnitTests.Entities;

[TestFixture]
internal class KeyGeneratorTest
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
        KeyGenerator generator = new();

        // Act
        string key = generator.GenerateKey(length);

        // Assert
        Assert.That(key, Has.Length.EqualTo(expectedLength));
    }

    [Test]
    public void ShouldGenerateUniqueKeys()
    {
        // Arrange
        KeyGenerator generator = new();

        // Act
        string key1 = generator.GenerateKey(256);
        string key2 = generator.GenerateKey(256);

        // Assert
        Assert.That(key1, Is.Not.EqualTo(key2));
    }
}
