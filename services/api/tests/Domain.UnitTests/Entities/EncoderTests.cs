using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Domain.Entities;
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
    [TestCase(512, 512)]
    [TestCase(1000, 1000)]
    public void EncodeAsync_ShouldNotThrowForMessageWithMaximumLength(int width, int height)
    {
        // Arrange
        Image<Rgb24> coverImage = new(width, height);

        // Act
        async Task Action()
        {
            Encoder encoder = new(coverImage, 42);
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
    [TestCase(512, 512)]
    public void EncodeAsync_ShouldThrowWhenTheMessageIsTooLong(int width, int height)
    {
        // Arrange
        Image<Rgb24> coverImage = new(width, height);

        // Act
        async Task Action()
        {
            Encoder encoder = new(coverImage, 42);
            int messageLength = coverImage.Width * coverImage.Height * 3 + 1;

            Pipe pipe = new();
            Task writing = WriteAsync(pipe.Writer, messageLength);
            Task reading = encoder.EncodeAsync(pipe.Reader);

            await Task.WhenAll(writing, reading);
        }

        // Assert
        Assert.That(Action, Throws.InvalidOperationException);
    }

    private static async Task WriteAsync(PipeWriter writer, int messageLength)
    {
        int leftToWrite = messageLength;
        int bytesWritten = 0;

        while (true)
        {
            Memory<byte> buffer = writer.GetMemory();
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
                await writer.FlushAsync();
                break;
            }
        }

        await writer.CompleteAsync();
    }
}
