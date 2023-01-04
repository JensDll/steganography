using System.Buffers;
using System.IO.Pipelines;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Encoder : ImageCodec
{
    public Encoder(Image<Rgb24> coverImage, int seed) : base(coverImage, seed) { }

    public async Task EncodeAsync(PipeReader pipeReader, CancellationToken cancellationToken)
    {
        while (true)
        {
            ReadResult result = await pipeReader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = result.Buffer;

            if (buffer.IsEmpty || result.IsCanceled)
            {
                break;
            }

            ProcessMessage(ref buffer);

            pipeReader.AdvanceTo(buffer.End, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await pipeReader.CompleteAsync();
    }

    private void ProcessMessage(ref ReadOnlySequence<byte> message)
    {
        SequenceReader<byte> reader = new(message);

        reader.TryRead(out byte currentByte);

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
                        byte* pixelValues = (byte*)pixel;

                        while (PixelIdx < 3)
                        {
                            pixelValues[PixelIdx] = (byte)((pixelValues[PixelIdx++] & PixelValueMask) |
                                                           (((currentByte >> ByteShift++) & 1) << BitPosition));

                            if (ByteShift != 8)
                            {
                                continue;
                            }

                            ByteShift = 0;

                            if (!reader.TryRead(out currentByte))
                            {
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
    }
}
