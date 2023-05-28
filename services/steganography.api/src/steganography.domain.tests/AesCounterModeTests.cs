using System;
using NUnit.Framework;

namespace steganography.domain.tests;

[TestFixture]
internal class AesCounterModeTests
{
    [Test]
    public void PlainTextAreEqualAfterTransformation([Random(500, 100_100, 5)] int length)
    {
        // Arrange
        AesCounterMode encryptor = new();
        AesCounterMode decryptor = new(encryptor.Key, encryptor.InitializationValue);

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
    public void PlainTextAreEqualAfterBlockwiseTransformation()
    {
        // Arrange
        AesCounterMode encryptor = new();
        AesCounterMode decryptor = new(encryptor.Key, encryptor.InitializationValue);

        const int length = 4096;
        Span<byte> plaintextIn = new byte[length];
        TestContext.CurrentContext.Random.NextBytes(plaintextIn);
        Span<byte> ciphertext = new byte[length];
        Span<byte> plaintextOut = new byte[length];

        // Act
        encryptor.Transform(plaintextIn[..500], ciphertext); // Transform the first 500 bytes
        encryptor.Transform(plaintextIn[500..1000], ciphertext[500..]); // the next 500
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
    public void PlainTextAreDifferentWhenTheKeyChanges(int length)
    {
        // Arrange
        AesCounterMode encryptor = new();
        byte[] alteredKey = encryptor.Key;
        ++alteredKey[0];
        AesCounterMode decryptor = new(alteredKey, encryptor.InitializationValue);

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
    public void EqualPlaintextShouldResultInUnequalCiphertext()
    {
        // Arrange
        AesCounterMode encryptor = new();

        byte[] plaintextIn = new byte[32];
        byte[] ciphertext = new byte[32];

        // Act
        encryptor.Transform(plaintextIn, ciphertext);

        // Assert
        // If the same input is used for both blocks (i.e. by not changing the counter), this would fail
        Assert.That(ciphertext[..16], Is.Not.EqualTo(ciphertext[16..]));
    }
}
