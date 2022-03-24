using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Services;
using NUnit.Framework;

namespace Domain.UnitTests.Services;

[TestFixture]
internal class KeyServiceTests
{
    [TestCaseSource(nameof(TestData))]
    public void ToBase64_TryParse_AllKeyPartsShouldMatch(MessageType inMessageType, ushort inSeed,
        int inMessageLength, byte[] inKey, byte[] inIv)
    {
        // Arrange
        KeyService keyService = new();

        // Act
        string base64Key = keyService.ToBase64(inMessageType, inSeed, inMessageLength, inKey, inIv);
        bool success = keyService.TryParse(base64Key, out MessageType outMessageType, out ushort outSeed,
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
    public void TryParse_ShouldFailForInvalidKey()
    {
        // Arrange
        KeyService keyService = new();

        // Act
        bool success = keyService.TryParse("invalid", out _, out _, out _, out _, out _);

        // Assert
        Assert.That(success, Is.False);
    }

    private static IEnumerable<object[]> TestData()
    {
        foreach (int _ in Enumerable.Range(1, 5))
        {
            MessageType messageType = (MessageType) TestContext.CurrentContext.Random.Next(0, 2);
            ushort seed = TestContext.CurrentContext.Random.NextUShort();
            int messageLength = TestContext.CurrentContext.Random.Next();
            byte[] key = new byte[32];
            byte[] iv = new byte[12];
            TestContext.CurrentContext.Random.NextBytes(key);
            TestContext.CurrentContext.Random.NextBytes(iv);
            yield return new object[] {messageType, seed, messageLength, key, iv};
        }
    }
}
