using Domain.Enums;
using Domain.Services;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace Domain.UnitTests.Services;

[TestFixture]
internal class KeyServiceTests
{
    [Test]
    public void GenerateKey_ShouldGenerate48byteKey()
    {
        //Arrange
        KeyService keyService = new();

        // Act
        string key = keyService.GenerateKey();

        // Assert
        Assert.That(key, Has.Length.EqualTo(64));
    }

    [Test]
    public void Generate_ShouldBeUnique()
    {
        // Arrange
        KeyService keyService = new();

        // Act
        string key1 = keyService.GenerateKey();
        string key2 = keyService.GenerateKey();

        // Assert
        Assert.That(key1, Is.Not.EqualTo(key2));
    }

    [TestCase(MessageType.Text, (ushort) 10, 42)]
    [TestCase(MessageType.Text, (ushort) 20, 100)]
    [TestCase(MessageType.Binary, (ushort) 30, 100_000)]
    public void TryParseKey_ShouldParseKeyComponents(MessageType inMessageType, ushort inSeed, int inMessageLength)
    {
        // Arrange
        KeyService keyService = new();
        string inKey = keyService.GenerateKey();
        string inKeyMeta = keyService.AddMetaData(inKey, inMessageType, inSeed, inMessageLength);

        // Act
        bool success = keyService.TryParse(inKeyMeta, out MessageType outMessageType, out ushort outSeed,
            out int outMessageLength, out StringSegment outKey);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(inMessageType, Is.EqualTo(outMessageType));
        Assert.That(inSeed, Is.EqualTo(outSeed));
        Assert.That(inMessageLength, Is.EqualTo(outMessageLength));
        Assert.That(inKey.Length, Is.EqualTo(outKey.Length));
        Assert.That(inKey, Is.EqualTo(outKey));
    }

    [Test]
    public void TryParseKey_ShouldFailForInvalidKey()
    {
        // Arrange
        KeyService keyService = new();

        // Act
        bool success = keyService.TryParse("invalid", out _, out _, out _, out _);

        // Assert
        Assert.That(success, Is.False);
    }
}
