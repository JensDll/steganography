using System.IO.Pipelines;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Decoder : CodecBase
{
    private readonly int _messageLength;
    private readonly AesCounterMode _aes;
    private int _bytesRead;

    public Decoder(Image<Rgb24> coverImage, ushort seed, int messageLength, AesCounterMode aes) :
        base(coverImage, seed)
    {
        _messageLength = messageLength;
        _aes = aes;
    }

    public async Task DecodeAsync(PipeWriter writer)
    {
        await WriteMessageAsync(writer, _messageLength);
        ReturnPermutations();
    }

    public async Task DecodeAsync(PipeWriter writer, int messageLength)
    {
        _bytesRead += messageLength;
        await WriteMessageAsync(writer, messageLength);
        if (_bytesRead >= _messageLength)
        {
            ReturnPermutations();
        }
    }

    public bool TryReadNextFileInfo(out string fileName, out int fileLength)
    {
        fileName = string.Empty;
        fileLength = 0;

        if (_bytesRead >= _messageLength)
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[8];
        ReadNextBytes(buffer);

        fileLength = BitConverter.ToInt32(buffer[..4]);
        int fileNameLength = BitConverter.ToInt32(buffer[4..]);

        Span<byte> fileNameBytes = stackalloc byte[fileNameLength];
        ReadNextBytes(fileNameBytes);

        fileName = Encoding.UTF8.GetString(fileNameBytes);
        _bytesRead += fileNameLength + 8;

        return true;
    }

    private void ReadNextBytes(Span<byte> buffer)
    {
        int bytesRead = 0;

        while (BitPosition < 8)
        {
            while (StartPermutationIdx < StartPermutationCount)
            {
                while (PermutationIdx < PermutationCount)
                {
                    int y = Math.DivRem(Permutation[PermutationIdx], CoverImage.Width, out int x);
                    Span<Rgb24> row = CoverImage.DangerousGetPixelRowMemory(y).Span;

                    unsafe
                    {
                        fixed (Rgb24* pixel = &row[x])
                        {
                            byte* pixelValues = (byte*) pixel;

                            while (PixelIdx < 3)
                            {
                                int bit = (pixelValues[PixelIdx] >> BitPosition) & 1;
                                buffer[bytesRead] |= (byte) (bit << ByteShift++);

                                ++PixelIdx;

                                if (ByteShift != 8)
                                {
                                    continue;
                                }

                                ByteShift = 0;

                                if (++bytesRead == buffer.Length)
                                {
                                    _aes.Transform(buffer, buffer);
                                    return;
                                }
                            }

                            PixelIdx = 0;
                        }
                    }

                    ++PermutationIdx;
                }

                NextPermutation();
            }

            StartPermutationIdx = 0;
            ++BitPosition;
            PixelValueMask <<= 1;
        }
    }

    private async Task WriteMessageAsync(PipeWriter pipeWriter, int messageLength)
    {
        bool done = messageLength == 0;

        while (true)
        {
            CoverImage.ProcessPixelRows(accessor =>
            {
                int bytesRead = 0;
                Span<byte> buffer = pipeWriter.GetSpan(512);
                buffer[bytesRead] = 0;

                while (BitPosition < 8)
                {
                    while (StartPermutationIdx < StartPermutationCount)
                    {
                        while (PermutationIdx < PermutationCount)
                        {
                            int y = Math.DivRem(Permutation[PermutationIdx], accessor.Width, out int x);
                            Span<Rgb24> row = accessor.GetRowSpan(y);

                            unsafe
                            {
                                fixed (Rgb24* pixel = &row[x])
                                {
                                    byte* pixelValues = (byte*) pixel;

                                    while (PixelIdx < 3)
                                    {
                                        int bit = (pixelValues[PixelIdx] >> BitPosition) & 1;
                                        buffer[bytesRead] |= (byte) (bit << ByteShift++);

                                        ++PixelIdx;

                                        if (ByteShift != 8)
                                        {
                                            continue;
                                        }

                                        ByteShift = 0;
                                        done = --messageLength == 0;

                                        if (++bytesRead == buffer.Length || done)
                                        {
                                            _aes.Transform(buffer[..bytesRead], buffer);
                                            pipeWriter.Advance(bytesRead);
                                            return;
                                        }

                                        buffer[bytesRead] = 0;
                                    }

                                    PixelIdx = 0;
                                }
                            }

                            ++PermutationIdx;
                        }

                        NextPermutation();
                    }

                    StartPermutationIdx = 0;
                    ++BitPosition;
                    PixelValueMask <<= 1;
                }
            });

            await pipeWriter.FlushAsync();

            if (done)
            {
                break;
            }
        }

        await pipeWriter.CompleteAsync();
    }
}
