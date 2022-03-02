using System.Text;
using Domain.Extensions;
using Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Decoder : IDecoder
{
    public byte[] Decode(Image<Rgb24> coverImage)
    {
        int messageLength = DecodeMessageLength(coverImage);
        ushort seed = DecodeSeed(coverImage);

        byte[] result = new byte[messageLength];

        coverImage.ProcessPixelRows(accessor =>
        {
            int numPixel = coverImage.Width * coverImage.Height;
            int[] permutation = new Random(seed).Permutation(2, numPixel - 4);

            int i = 0;
            byte byteShift = 0;

            for (byte bitPosition = 0; bitPosition <= 7; ++bitPosition)
            {
                foreach (int n in permutation)
                {
                    int y = Math.DivRem(n, coverImage.Width, out int x);
                    Span<Rgb24> row = accessor.GetRowSpan(y);
                    ref Rgb24 pixel = ref row[x];

                    DecodeNextBit(pixel.R, bitPosition, ref byteShift, ref result[i], ref i);

                    if (i == messageLength)
                    {
                        return;
                    }

                    DecodeNextBit(pixel.G, bitPosition, ref byteShift, ref result[i], ref i);

                    if (i == messageLength)
                    {
                        return;
                    }

                    DecodeNextBit(pixel.B, bitPosition, ref byteShift, ref result[i], ref i);

                    if (i == messageLength)
                    {
                        return;
                    }
                }
            }
        });

        return result;
    }

    public List<DecodedItem> ParseBytes(byte[] data, out bool isText)
    {
        isText = false;

        ReadOnlySpan<byte> dataSpan = data;
        List<DecodedItem> decodedItems = new();

        int position = 0;

        while (position < dataSpan.Length)
        {
            int fileNameLength = BitConverter.ToInt32(dataSpan[position..(position += 4)]);
            string fileName = Encoding.ASCII.GetString(dataSpan[position..(position += fileNameLength)]);
            int fileLength = BitConverter.ToInt32(dataSpan[position..(position += 4)]);

            decodedItems.Add(new DecodedItem
            {
                Name = fileName,
                Data = dataSpan[position..(position += fileLength)].ToArray()
            });
        }

        return decodedItems;
    }

    private static void DecodeNextBit(byte pixelValue, byte bitPosition, ref byte byteShift,
        ref byte currentByte, ref int i)
    {
        int bit = (pixelValue >> bitPosition) & 1;
        currentByte |= (byte) (bit << byteShift++);

        if (byteShift == 8)
        {
            ++i;
            byteShift = 0;
        }
    }

    private static int DecodeMessageLength(Image<Rgb24> coverImage)
    {
        int result = 0;

        coverImage.ProcessPixelRows(accessor =>
        {
            Span<Rgb24> firstRow = accessor.GetRowSpan(0);
            Span<Rgb24> lastRow = accessor.GetRowSpan(coverImage.Height - 1);

            ref Rgb24 p1 = ref firstRow[0];
            ref Rgb24 p2 = ref firstRow[1];
            ref Rgb24 p3 = ref lastRow[^2];
            ref Rgb24 p4 = ref lastRow[^1];

            const byte low = 0b_0000_1111;

            // 12 bit RGB
            result |= p1.R & low;
            result |= (p1.G & low) << 4;
            result |= (p1.B & low) << 8;

            // 4 bit R
            result |= (p2.R & low) << 12;

            // 4 bit R
            result |= (p3.R & low) << 16;

            // 12 bit RGB
            result |= (p4.R & low) << 20;
            result |= (p4.G & low) << 24;
            result |= (p4.B & low) << 28;
        });

        return result;
    }

    private static ushort DecodeSeed(Image<Rgb24> coverImage)
    {
        int result = 0;

        coverImage.ProcessPixelRows(accessor =>
        {
            Span<Rgb24> firstRow = accessor.GetRowSpan(0);
            Span<Rgb24> lastRow = accessor.GetRowSpan(coverImage.Height - 1);

            ref Rgb24 p1 = ref firstRow[1];
            ref Rgb24 p2 = ref lastRow[^2];

            const byte low = 0b_0000_1111;

            // 8 bit GB
            result |= p1.G & low;
            result |= (p1.B & low) << 4;

            // 8 bit GB
            result |= (p2.G & low) << 8;
            result |= (p2.B & low) << 12;
        });

        return (ushort) result;
    }
}
