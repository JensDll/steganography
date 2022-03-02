using Domain.Extensions;
using Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Encoder : IEncoder
{
    private readonly Random _prng = new();

    public void Encode(Image<Rgb24> coverImage, byte[] data)
    {
        ushort seed = (ushort) _prng.Next();

        EncodeMessageLength(coverImage, data.Length);
        EncodeSeed(coverImage, seed);

        coverImage.ProcessPixelRows(accessor =>
        {
            int numPixel = coverImage.Width * coverImage.Height;
            int[] permutation = new Random(seed).Permutation(2, numPixel - 4);

            int i = 0;
            byte byteShift = 0;
            byte currentByte = data[i];
            byte pixelValueMask = 1;

            for (byte bitPosition = 0; bitPosition <= 7; ++bitPosition, pixelValueMask <<= 1)
            {
                foreach (int n in permutation)
                {
                    int y = Math.DivRem(n, coverImage.Width, out int x);
                    Span<Rgb24> row = accessor.GetRowSpan(y);
                    ref Rgb24 pixel = ref row[x];

                    bool done = EncodeNextBit(ref pixel.R, ref currentByte, ref byteShift, ref i,
                        data,
                        bitPosition,
                        pixelValueMask);

                    if (done)
                    {
                        return;
                    }

                    done = EncodeNextBit(ref pixel.G, ref currentByte, ref byteShift, ref i,
                        data,
                        bitPosition,
                        pixelValueMask);

                    if (done)
                    {
                        return;
                    }

                    done = EncodeNextBit(ref pixel.B, ref currentByte, ref byteShift, ref i,
                        data,
                        bitPosition,
                        pixelValueMask);

                    if (done)
                    {
                        return;
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(data), data, "Data is too long for the image");
        });
    }

    private static bool EncodeNextBit(ref byte pixelValue, ref byte currentByte, ref byte byteShift, ref int i,
        IReadOnlyList<byte> data, byte bitPosition, byte pixelValueMask)
    {
        int bit = (currentByte >> byteShift++) & 1;
        pixelValue = (byte) ((pixelValue & ~pixelValueMask) | (bit << bitPosition));

        if (byteShift != 8)
        {
            return false;
        }

        if (++i >= data.Count)
        {
            return true;
        }

        currentByte = data[i];
        byteShift = 0;

        return false;
    }

    private static void EncodeMessageLength(Image<Rgb24> coverImage, int length)
    {
        coverImage.ProcessPixelRows(accessor =>
        {
            Span<Rgb24> firstRow = accessor.GetRowSpan(0);
            Span<Rgb24> lastRow = accessor.GetRowSpan(coverImage.Height - 1);

            ref Rgb24 p1 = ref firstRow[0];
            ref Rgb24 p2 = ref firstRow[1];
            ref Rgb24 p3 = ref lastRow[^2];
            ref Rgb24 p4 = ref lastRow[^1];

            const byte low = 0b_0000_1111;
            const byte high = 0b_1111_0000;

            // 12 bit RGB
            p1.R = (byte) ((p1.R & high) | (length & low));
            p1.G = (byte) ((p1.G & high) | ((length >> 4) & low));
            p1.B = (byte) ((p1.B & high) | ((length >> 8) & low));

            // 4 bit R
            p2.R = (byte) ((p2.R & high) | ((length >> 12) & low));

            // 4 bit R
            p3.R = (byte) ((p3.R & high) | ((length >> 16) & low));

            // 12 bit RGB
            p4.R = (byte) ((p4.R & high) | ((length >> 20) & low));
            p4.G = (byte) ((p4.G & high) | ((length >> 24) & low));
            p4.B = (byte) ((p4.B & high) | ((length >> 28) & low));
        });
    }

    private static void EncodeSeed(Image<Rgb24> coverImage, ushort seed)
    {
        coverImage.ProcessPixelRows(accessor =>
        {
            Span<Rgb24> firstRow = accessor.GetRowSpan(0);
            Span<Rgb24> lastRow = accessor.GetRowSpan(coverImage.Height - 1);

            ref Rgb24 p1 = ref firstRow[1];
            ref Rgb24 p2 = ref lastRow[^2];

            const byte low = 0b_0000_1111;
            const byte high = 0b_1111_0000;

            // 8 bit GB
            p1.G = (byte) ((p1.G & high) | (seed & low));
            p1.B = (byte) ((p2.B & high) | ((seed >> 4) & low));

            // 8 bit GB
            p2.G = (byte) ((p2.G & high) | ((seed >> 8) & low));
            p2.B = (byte) ((p2.B & high) | ((seed >> 12) & low));
        });
    }
}
