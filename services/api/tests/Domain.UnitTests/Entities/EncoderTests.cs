using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Exceptions;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.UnitTests.Entities;

[TestFixture]
internal class EncoderTests
{
    [TestCase(8, 8)]
    [TestCase(64, 64)]
    [TestCase(128, 128)]
    [TestCase(256, 256)]
    [TestCase(500, 500)]
    [TestCase(1000, 1000)]
    [TestCase(1500, 1500)]
    public void Should_not_throw_for_message_with_maximum_length(int width, int height)
    {
        // Arrange
        Image<Rgb24> coverImage = new(width, height);

        // Act
        async Task Action()
        {
            CancellationTokenSource cancelSource = new();
            Encoder encoder = new(coverImage, 42, cancelSource);
            int messageLength = coverImage.Width * coverImage.Height * 3;

            Pipe pipe = new();
            Task writing = WriteAsync(pipe.Writer, messageLength);
            Task reading = encoder.EncodeAsync(pipe.Reader);

            await Task.WhenAll(writing, reading);
        }

        // Assert
        Assert.That(Action, Throws.Nothing);
    }

    [TestCase(8, 8)]
    [TestCase(64, 64)]
    [TestCase(128, 128)]
    [TestCase(256, 256)]
    [TestCase(500, 500)]
    [TestCase(1000, 1000)]
    [TestCase(1500, 1500)]
    public void Should_throw_for_too_long_messages(int width, int height)
    {
        // Arrange
        Image<Rgb24> coverImage = new(width, height);

        // Act
        async Task Action()
        {
            CancellationTokenSource cancelSource = new();
            Encoder encoder = new(coverImage, 42, cancelSource);
            int messageLength = coverImage.Width * coverImage.Height * 3 + 1;

            Pipe pipe = new();
            Task writing = WriteAsync(pipe.Writer, messageLength);
            Task reading = encoder.EncodeAsync(pipe.Reader);

            await Task.WhenAll(writing, reading);
        }

        // Assert
        Assert.That(Action, Throws.TypeOf<MessageTooLongException>());
    }

    private static async Task WriteAsync(PipeWriter writer, int messageLength)
    {
        int leftToWrite = messageLength;
        int bytesWritten = 0;

        while (true)
        {
            Memory<byte> buffer = writer.GetMemory(512);
            Random.Shared.NextBytes(buffer.Span);
            bytesWritten += buffer.Length;

            if (bytesWritten < messageLength)
            {
                leftToWrite -= buffer.Length;
                writer.Advance(buffer.Length);
                await writer.FlushAsync();
            }
            else
            {
                writer.Advance(leftToWrite);
                break;
            }
        }

        await writer.CompleteAsync();
    }
}
