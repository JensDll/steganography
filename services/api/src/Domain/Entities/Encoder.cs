using Domain.Extensions;
using Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Encoder : IEncoder
{
    private const int _permutationSize = 2048;

    public void Encode(Image<Rgb24> coverImage, byte[] message, ushort seed)
    {
        coverImage.ProcessPixelRows(accessor =>
        {
            Random prng = new(seed);
            int pixelNumber = accessor.Width * accessor.Height;
            int step = pixelNumber / _permutationSize;
            step = step == 0 ? 1 : step;

            int messagePosition = 0;
            byte bitPosition = 0;
            byte currentByte = message[messagePosition];
            byte byteShift = 0;
            byte pixelValueMask = 1;

            for (; bitPosition <= 7; ++bitPosition, pixelValueMask <<= 1)
            {
                for (int start = 0; start < step; ++start)
                {
                    int[] permutation = prng.Permutation(start, pixelNumber - 1, step);

                    foreach (int n in permutation)
                    {
                        int y = Math.DivRem(n, accessor.Width, out int x);
                        Span<Rgb24> row = accessor.GetRowSpan(y);
                        ref Rgb24 pixel = ref row[x];

                        bool done = EncodeNextBit(
                            message: message,
                            bitPosition: bitPosition,
                            pixelValueMask: pixelValueMask,
                            messagePosition: ref messagePosition,
                            pixelValue: ref pixel.R,
                            currentByte: ref currentByte,
                            byteShift: ref byteShift
                        );

                        if (done)
                        {
                            return;
                        }

                        done = EncodeNextBit(
                            message: message,
                            bitPosition: bitPosition,
                            pixelValueMask: pixelValueMask,
                            messagePosition: ref messagePosition,
                            pixelValue: ref pixel.G,
                            currentByte: ref currentByte,
                            byteShift: ref byteShift
                        );

                        if (done)
                        {
                            return;
                        }

                        done = EncodeNextBit(
                            message: message,
                            bitPosition: bitPosition,
                            pixelValueMask: pixelValueMask,
                            messagePosition: ref messagePosition,
                            pixelValue: ref pixel.B,
                            currentByte: ref currentByte,
                            byteShift: ref byteShift
                        );

                        if (done)
                        {
                            return;
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(message), message, "Message is too long for the image");
        });
    }

    private static bool EncodeNextBit(
        IReadOnlyList<byte> message,
        byte bitPosition,
        byte pixelValueMask,
        ref int messagePosition,
        ref byte pixelValue,
        ref byte currentByte,
        ref byte byteShift
    )
    {
        int bit = (currentByte >> byteShift++) & 1;
        pixelValue = (byte) ((pixelValue & ~pixelValueMask) | (bit << bitPosition));

        if (byteShift != 8)
        {
            return false;
        }

        if (++messagePosition >= message.Count)
        {
            return true;
        }

        currentByte = message[messagePosition];
        byteShift = 0;

        return false;
    }
}
