using System;
using Domain.Entities;
using NUnit.Framework;

namespace Domain.UnitTests.Entities;

[TestFixture]
internal class AesCounterModeTests
{
    [Test]
    public void Transform_ShouldEqualAfterTransformation([Random(500, 100_100, 5)] int length)
    {
        // Arrange
        AesCounterMode encryptor = new();
        AesCounterMode decryptor = new(encryptor.Key, encryptor.IV);

        byte[] plaintextIn = new byte[length];
        TestContext.CurrentContext.Random.NextBytes(plaintextIn);
        byte[] ciphertext = new byte[length];
        byte[] plaintextOut = new byte[length];

        // Act
        encryptor.Transform(plaintextIn, ciphertext);
        decryptor.Transform(ciphertext, plaintextOut);

        // Assert
        Assert.That(plaintextIn, Is.Not.EqualTo(ciphertext));
        Assert.That(plaintextOut, Is.EqualTo(plaintextIn));
    }

    [Test]
    public void Transform_InBlocks_ShouldEqualAfterTransformation()
    {
        // Arrange
        AesCounterMode encryptor = new();
        AesCounterMode decryptor = new(encryptor.Key, encryptor.IV);

        const int length = 4096;
        Span<byte> plaintextIn = new byte[length];
        TestContext.CurrentContext.Random.NextBytes(plaintextIn);
        Span<byte> ciphertext = new byte[length];
        Span<byte> plaintextOut = new byte[length];

        // Act
        encryptor.Transform(plaintextIn[..500], ciphertext);
        encryptor.Transform(plaintextIn[500..1000], ciphertext[500..]);
        encryptor.Transform(plaintextIn[1000..], ciphertext[1000..]);
        decryptor.Transform(ciphertext, plaintextOut);

        // Assert
        Assert.That(plaintextIn.ToArray(), Is.Not.EqualTo(ciphertext.ToArray()));
        Assert.That(plaintextOut.ToArray(), Is.EqualTo(plaintextIn.ToArray()));
    }
}
