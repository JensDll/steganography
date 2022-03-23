using Domain.Enums;
using Domain.Services;
using NUnit.Framework;

namespace Domain.UnitTests.Services;

[TestFixture]
internal class KeyServiceTests
{
    [Test]
    public void ShouldGenerateAesKeyAndIv()
    {
        //Arrange
        KeyService keyService = new();

        // Act
        (string base64Key, byte[] key, byte[] iV) = keyService.GenerateKey();

        // Assert
        Assert.That(base64Key, Has.Length.EqualTo(64));
        Assert.That(key, Has.Length.EqualTo(32));
        Assert.That(iV, Has.Length.EqualTo(16));
    }

    [Test]
    public void ShouldBeUnique()
    {
        // Arrange
        KeyService keyService = new();

        // Act
        (string base64Key1, _, _) = keyService.GenerateKey();
        (string base64Key2, _, _) = keyService.GenerateKey();

        // Assert
        Assert.That(base64Key1, Is.Not.EqualTo(base64Key2));
    }

    [TestCase(MessageType.Text, (ushort) 10, 42)]
    [TestCase(MessageType.Text, (ushort) 20, 100)]
    [TestCase(MessageType.Binary, (ushort) 30, 100_000)]
    public void ShouldAddAndParseKeyComponents(MessageType inMessageType, ushort inSeed, int inMessageLength)
    {
        // Arrange
        KeyService keyService = new();
        (string inBase64Key, byte[] inKey, byte[] inIv) = keyService.GenerateKey();
        string inKeyMeta = keyService.AddMetaData(inBase64Key, inMessageType, inSeed, inMessageLength);

        // Act
        bool success = keyService.TryParse(inKeyMeta, out MessageType outMessageType, out ushort outSeed,
            out int outMessageLength, out byte[] outKey, out byte[] outIv);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(outMessageType, Is.EqualTo(inMessageType));
        Assert.That(outSeed, Is.EqualTo(inSeed));
        Assert.That(outMessageLength, Is.EqualTo(inMessageLength));
        Assert.That(outKey, Is.EqualTo(inKey));
        Assert.That(outIv, Is.EqualTo(inIv));
    }

    [Test]
    public void ShouldFailForInvalidKey()
    {
        // Arrange
        KeyService keyService = new();

        // Act
        bool success = keyService.TryParse("invalid", out _, out _, out _, out _, out _);

        // Assert
        Assert.That(success, Is.False);
    }
}
