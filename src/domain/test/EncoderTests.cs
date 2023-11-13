using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace domain.test;

[TestFixture]
internal sealed class EncoderTests
{
    [TestCase(8, 8)]
    [TestCase(64, 64)]
    [TestCase(128, 128)]
    [TestCase(256, 256)]
    [TestCase(512, 512)]
    [TestCase(1000, 1000)]
    public void EncodeAsync_Should_Not_Throw_For_Message_With_Maximum_Length(int width, int height)
    {
        Assert.That(Action, Throws.Nothing);
        return;

        async Task Action()
        {
            using Image<Rgb24> coverImage = new(width, height);

            ImageEncoder imageEncoder = new(coverImage, 42);
            int messageLength = coverImage.Width * coverImage.Height * 3;

            Pipe pipe = new();
            Task writing = WriteAsync(pipe.Writer, messageLength);
            Task reading = imageEncoder.EncodeAsync(pipe.Reader, CancellationToken.None);

            await Task.WhenAll(writing, reading);
        }
    }

    [TestCase(8, 8)]
    [TestCase(64, 64)]
    [TestCase(128, 128)]
    [TestCase(256, 256)]
    [TestCase(512, 512)]
    public void EncodeAsync_Should_Throw_When_The_Message_Is_Too_Long(int width, int height)
    {
        Assert.That(Action, Throws.InvalidOperationException);
        return;

        async Task Action()
        {
            using Image<Rgb24> coverImage = new(width, height);

            ImageEncoder imageEncoder = new(coverImage, 42);
            int messageLength = coverImage.Width * coverImage.Height * 3 + 1;

            Pipe pipe = new();
            Task writing = WriteAsync(pipe.Writer, messageLength);
            Task reading = imageEncoder.EncodeAsync(pipe.Reader, CancellationToken.None);

            await Task.WhenAll(writing, reading);
        }
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
