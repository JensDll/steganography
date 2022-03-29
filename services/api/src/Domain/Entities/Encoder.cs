using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Encoder : CodecBase
{
    private readonly CancellationTokenSource _cancelSource;

    public Encoder(Image<Rgb24> coverImage, int seed, CancellationTokenSource cancelSource) : base(coverImage, seed)
    {
        _cancelSource = cancelSource;
    }

    public async Task EncodeAsync(PipeReader pipeReader)
    {
        long consumed = 0;

        while (true)
        {
            ReadResult result = await pipeReader.ReadAsync(_cancelSource.Token);
            ReadOnlySequence<byte> buffer = result.Buffer;

            ProcessMessage(ref buffer, ref consumed);
            pipeReader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await pipeReader.CompleteAsync();
    }

    private void ProcessMessage(ref ReadOnlySequence<byte> message, ref long consumed)
    {
        SequenceReader<byte> reader = new(message);
        reader.Advance(consumed);

        if (reader.Length > CoverImageCapacity)
        {
            _cancelSource.Cancel();
            throw new InvalidOperationException("Message is too long for the cover image");
        }

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
                        byte* pixelValues = (byte*) pixel;

                        while (PixelIdx < 3)
                        {
                            pixelValues[PixelIdx] = (byte) ((pixelValues[PixelIdx++] & PixelValueMask) |
                                                            (((currentByte >> ByteShift++) & 1) << BitPosition));

                            if (ByteShift != 8)
                            {
                                continue;
                            }

                            ByteShift = 0;

                            if (!reader.TryRead(out currentByte))
                            {
                                consumed = reader.Consumed;
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

        Debug.Fail("Should not be reached");
    }
}
