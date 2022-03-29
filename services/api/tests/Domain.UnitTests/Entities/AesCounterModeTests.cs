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
        Assert.That(ciphertext, Is.Not.EqualTo(plaintextIn));
        Assert.That(ciphertext, Is.Not.EqualTo(plaintextOut));
        Assert.That(plaintextIn, Is.EqualTo(plaintextOut));
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
        Assert.That(ciphertext.ToArray(), Is.Not.EqualTo(plaintextIn.ToArray()));
        Assert.That(ciphertext.ToArray(), Is.Not.EqualTo(plaintextOut.ToArray()));
        Assert.That(plaintextIn.ToArray(), Is.EqualTo(plaintextOut.ToArray()));
    }

    [Test]
    public void Transform_DifferentWhenKeyChanges()
    {
        // Arrange
        AesCounterMode encryptor = new();
        byte[] key = encryptor.Key;
        key[0]++;
        AesCounterMode decryptor = new(key, encryptor.IV);

        const int length = 4;
        byte[] plaintextIn = new byte[length];
        TestContext.CurrentContext.Random.NextBytes(plaintextIn);
        byte[] ciphertext = new byte[length];
        byte[] plaintextOut = new byte[length];

        // Act
        encryptor.Transform(plaintextIn, ciphertext);
        decryptor.Transform(ciphertext, plaintextOut);

        // Assert
        Assert.That(ciphertext, Is.Not.EqualTo(plaintextIn));
        Assert.That(ciphertext, Is.Not.EqualTo(plaintextOut));
        Assert.That(plaintextIn, Is.Not.EqualTo(plaintextOut));
    }
}
