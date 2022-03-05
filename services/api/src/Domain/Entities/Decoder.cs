using System.Text;
using Domain.Extensions;
using Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public class Decoder : IDecoder
{
    private const int _permutationSize = 2048;

    public List<DecodedItem> ParseMessage(ReadOnlySpan<byte> message, out bool isText)
    {
        isText = false;

        List<DecodedItem> decodedItems = new();

        int messagePosition = 0;

        while (messagePosition < message.Length)
        {
            int fileNameLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)]);
            isText = fileNameLength == 0;

            if (isText)
            {
                decodedItems.Add(new DecodedItem
                {
                    Name = string.Empty,
                    Data = message[messagePosition..].ToArray()
                });

                return decodedItems;
            }

            string fileName = Encoding.ASCII.GetString(message[messagePosition..(messagePosition += fileNameLength)]);
            int fileLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)]);

            decodedItems.Add(new DecodedItem
            {
                Name = fileName,
                Data = message[messagePosition..(messagePosition += fileLength)].ToArray()
            });
        }

        return decodedItems;
    }

    public byte[] Decode(Image<Rgb24> coverImage, ushort seed, int messageLength)
    {
        byte[] message = new byte[messageLength];

        coverImage.ProcessPixelRows(accessor =>
        {
            Random prng = new(seed);
            int pixelNumber = accessor.Width * accessor.Height;
            int step = pixelNumber / _permutationSize;

            int messagePosition = 0;
            byte bitPosition = 0;
            byte byteShift = 0;

            for (; bitPosition <= 7; ++bitPosition)
            {
                for (int start = 0; start < step; ++start)
                {
                    int[] permutation = prng.Permutation(start, pixelNumber - 1, step);

                    foreach (int n in permutation)
                    {
                        int y = Math.DivRem(n, accessor.Width, out int x);
                        Span<Rgb24> row = accessor.GetRowSpan(y);
                        ref Rgb24 pixel = ref row[x];

                        DecodeNextBit(
                            pixelValue: pixel.R,
                            bitPosition: bitPosition,
                            messagePosition: ref messagePosition,
                            byteShift: ref byteShift,
                            currentByte: ref message[messagePosition]
                        );

                        if (messagePosition == messageLength)
                        {
                            return;
                        }

                        DecodeNextBit(
                            pixelValue: pixel.G,
                            bitPosition: bitPosition,
                            messagePosition: ref messagePosition,
                            byteShift: ref byteShift,
                            currentByte: ref message[messagePosition]
                        );

                        if (messagePosition == messageLength)
                        {
                            return;
                        }

                        DecodeNextBit(
                            pixelValue: pixel.B,
                            bitPosition: bitPosition,
                            messagePosition: ref messagePosition,
                            byteShift: ref byteShift,
                            currentByte: ref message[messagePosition]
                        );

                        if (messagePosition == messageLength)
                        {
                            return;
                        }
                    }
                }
            }
        });

        return message;
    }

    private static void DecodeNextBit(
        byte pixelValue,
        byte bitPosition,
        ref byte byteShift,
        ref byte currentByte,
        ref int messagePosition
    )
    {
        int bit = (pixelValue >> bitPosition) & 1;
        currentByte |= (byte) (bit << byteShift++);

        if (byteShift == 8)
        {
            ++messagePosition;
            byteShift = 0;
        }
    }
}
