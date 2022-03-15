﻿using System.Text;
using Domain.Entities;
using Domain.Extensions;
using Domain.Interfaces;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Services;

public class DecodeService : CodecBase, IDecodeService
{
    private readonly ILogger _logger;

    public DecodeService(ILogger logger)
    {
        _logger = logger;
    }

    public byte[] Decode(Image<Rgb24> coverImage, ushort seed, int messageLength)
    {
        _logger.Information(
            "Decoding message with image size: (width: {Width}, height: {Height}), and message length: {MessageLength} bytes",
            coverImage.Width, coverImage.Height, messageLength);

        byte[] message = new byte[messageLength];

        coverImage.ProcessPixelRows(accessor =>
        {
            Random prng = new(seed);
            int pixelNumber = accessor.Width * accessor.Height;
            int step = pixelNumber / PermutationSize;
            step = step == 0 ? 1 : step;

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

    public List<DecodedItem> ParseMessage(ReadOnlyMemory<byte> message, out bool isText)
    {
        isText = false;

        List<DecodedItem> decodedItems = new();

        int messagePosition = 0;

        while (messagePosition < message.Length)
        {
            int fileNameLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)].Span);
            isText = fileNameLength == 0;

            if (isText)
            {
                decodedItems.Add(new DecodedItem
                {
                    Name = string.Empty,
                    Data = message[messagePosition..]
                });

                return decodedItems;
            }

            string fileName =
                Encoding.ASCII.GetString(message[messagePosition..(messagePosition += fileNameLength)].Span);
            int fileLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)].Span);

            decodedItems.Add(new DecodedItem
            {
                Name = fileName,
                Data = message[messagePosition..(messagePosition += fileLength)]
            });
        }

        return decodedItems;
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
