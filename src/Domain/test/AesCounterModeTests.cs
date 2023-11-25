using System;
using NUnit.Framework;

namespace Domain.Test;

[TestFixture]
internal sealed class AesCounterModeTests
{
    [Test]
    public void Plaintext_Are_Equal_After_Transformation([Random(500, 100_100, 5)] int length)
    {
        // Arrange
        using AesCounterMode encryptor = new();
        using AesCounterMode decryptor = new(encryptor.Key, encryptor.InitializationValue);

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
    public void Plaintext_Are_Equal_After_Blockwise_Transformation()
    {
        // Arrange
        using AesCounterMode encryptor = new();
        using AesCounterMode decryptor = new(encryptor.Key, encryptor.InitializationValue);

        const int length = 4096;
        Span<byte> plaintextIn = new byte[length];
        TestContext.CurrentContext.Random.NextBytes(plaintextIn);
        Span<byte> ciphertext = new byte[length];
        Span<byte> plaintextOut = new byte[length];

        // Act
        encryptor.Transform(plaintextIn[..500], ciphertext); // Transform the first 500-byte
        encryptor.Transform(plaintextIn[500..1000], ciphertext[500..]); // the next 500-byte
        encryptor.Transform(plaintextIn[1000..], ciphertext[1000..]); // and the rest
        decryptor.Transform(ciphertext, plaintextOut); // Decrypt all

        // Assert
        Assert.That(ciphertext.ToArray(), Is.Not.EqualTo(plaintextIn.ToArray()));
        Assert.That(ciphertext.ToArray(), Is.Not.EqualTo(plaintextOut.ToArray()));
        Assert.That(plaintextIn.ToArray(), Is.EqualTo(plaintextOut.ToArray()));
    }

    [TestCase(4)]
    [TestCase(8)]
    [TestCase(32)]
    public void Plaintext_Are_Different_When_The_Key_Changes(int length)
    {
        // Arrange
        using AesCounterMode encryptor = new();
        byte[] alteredKey = encryptor.Key;
        ++alteredKey[0];
        using AesCounterMode decryptor = new(alteredKey, encryptor.InitializationValue);

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

    [Test]
    public void Equal_Plaintext_Should_Result_In_Unequal_Ciphertext()
    {
        // Arrange
        using AesCounterMode encryptor = new();

        byte[] plaintextIn = new byte[32];
        byte[] ciphertext = new byte[32];

        // Act
        encryptor.Transform(plaintextIn, ciphertext);

        // Assert
        // If the same input is used for both blocks (i.e. by not changing the counter), this would fail
        Assert.That(ciphertext[..16], Is.Not.EqualTo(ciphertext[16..]));
    }
}
