using System.Buffers;
using System.IO.Pipelines;
using Domain.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Encoder : CodecBase
{
    private readonly CancellationTokenSource _cancelSource;

    public Encoder(Image<Rgb24> coverImage, ushort seed, CancellationTokenSource cancelSource) : base(coverImage, seed)
    {
        _cancelSource = cancelSource;
    }

    public async Task<int> EncodeAsync(PipeReader pipeReader)
    {
        long consumed = 0;

        while (true)
        {
            ReadResult result = await pipeReader.ReadAsync(_cancelSource.Token);
            ReadOnlySequence<byte> buffer = result.Buffer;

            pipeReader.AdvanceTo(buffer.Start, buffer.End);

            CoverImage.ProcessPixelRows(accessor =>
            {
                SequenceReader<byte> reader = new(buffer);

                if (reader.Length > CoverImageCapacity)
                {
                    ReturnPermutations();
                    _cancelSource.Cancel();
                    throw new MessageTooLongException("Message is too long for the cover image");
                }

                reader.Advance(consumed);
                reader.TryRead(out byte currentByte);

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
                                        int bit = (currentByte >> ByteShift++) & 1;
                                        pixelValues[PixelIdx] =
                                            (byte) ((pixelValues[PixelIdx] & ~PixelValueMask) | (bit << BitPosition));

                                        ++PixelIdx;

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

                    StartPermutationIdx = 0;
                    ++BitPosition;
                    PixelValueMask <<= 1;
                }
            });

            if (result.IsCompleted)
            {
                break;
            }
        }

        ReturnPermutations();

        await pipeReader.CompleteAsync();
        return (int) consumed;
    }
}
