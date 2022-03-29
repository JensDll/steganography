using System.Diagnostics;
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

    public Decoder(Image<Rgb24> coverImage, int seed, int messageLength, AesCounterMode aes) :
        base(coverImage, seed)
    {
        _messageLength = messageLength;
        _aes = aes;
    }

    public async Task DecodeAsync(PipeWriter writer)
    {
        await WriteMessageAsync(writer, _messageLength);
    }

    public async Task DecodeAsync(PipeWriter writer, int messageLength)
    {
        await WriteMessageAsync(writer, messageLength);
    }

    public IEnumerable<(string fileName, int fileLength)> DecodeFileInformation()
    {
        List<(string fileName, int fileLength)> fileInformation = new();
        int messageLength = _messageLength;

        Span<byte> buffer = stackalloc byte[6];
        Span<byte> fileNameBytes = stackalloc byte[256];

        while (true)
        {
            ReadNextBytes(buffer);
            int fileLength = BitConverter.ToInt32(buffer[..4]);
            short fileNameLength = BitConverter.ToInt16(buffer[4..]);

            if (fileLength < 0 || fileNameLength is <= 0 or > 256)
            {
                throw new InvalidOperationException("File name or file length is invalid");
            }

            messageLength -= 6 + fileLength + fileNameLength;

            if (messageLength < 0)
            {
                throw new InvalidOperationException("File name or file length is invalid");
            }

            Span<byte> fileNameBuffer = fileNameBytes[..fileNameLength];
            ReadNextBytes(fileNameBuffer);
            string fileName = Encoding.UTF8.GetString(fileNameBuffer);

            fileInformation.Add((fileName, fileLength));

            if (messageLength == 0)
            {
                break;
            }
        }

        return fileInformation;
    }

    private void ReadNextBytes(Span<byte> buffer)
    {
        int bytesRead = 0;
        buffer[0] = 0;

        Debug.Assert(ByteShift == 0);

        while (StartPermutationIdx <= StartPermutationCount)
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
                            buffer[bytesRead] |=
                                (byte) (((pixelValues[PixelIdx++] >> BitPosition) & 1) << ByteShift++);

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

                            buffer[bytesRead] = 0;
                        }

                        PixelIdx = 0;
                    }
                }

                ++PermutationIdx;
            }

            NextPermutation();
        }
    }

    private async Task WriteMessageAsync(PipeWriter pipeWriter, int messageLength)
    {
        bool done = false;

        while (true)
        {
            CoverImage.ProcessPixelRows(accessor =>
            {
                Debug.Assert(ByteShift == 0);

                int bytesRead = 0;
                Span<byte> buffer = pipeWriter.GetSpan();
                buffer[0] = 0;

                while (StartPermutationIdx <= StartPermutationCount)
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
                                    buffer[bytesRead] |=
                                        (byte) (((pixelValues[PixelIdx++] >> BitPosition) & 1) << ByteShift++);

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
