using System.Text;
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

    public unsafe byte[] Decode(Image<Rgb24> coverImage, ushort seed, int messageLength)
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

                        fixed (Rgb24* pixel = &row[x])
                        {
                            byte* pixelValues = (byte*)pixel;
                            for (int i = 0; i < 3; ++i)
                            {
                                int bit = (pixelValues[i] >> bitPosition) & 1;
                                message[messagePosition] |= (byte)(bit << byteShift++);

                                if (byteShift == 8)
                                {
                                    if (++messagePosition == messageLength)
                                    {
                                        return;
                                    }

                                    byteShift = 0;
                                }
                            }
                        }
                    }
                }
            }
        });

        return message;
    }

    public IEnumerable<DecodedFile> ParseFiles(ReadOnlyMemory<byte> message)
    {
        int messagePosition = 0;

        while (messagePosition < message.Length)
        {
            int fileNameLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)].Span);
            string fileName =
                Encoding.ASCII.GetString(message[messagePosition..(messagePosition += fileNameLength)].Span);
            int fileLength = BitConverter.ToInt32(message[messagePosition..(messagePosition += 4)].Span);

            yield return new DecodedFile
            {
                Name = fileName,
                Data = message[messagePosition..(messagePosition += fileLength)]
            };
        }
    }
}
