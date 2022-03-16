using Domain.Entities;
using Domain.Extensions;
using Domain.Interfaces;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Services;

public class EncodeService : CodecBase, IEncodeService
{
    private readonly ILogger _logger;

    public EncodeService(ILogger logger)
    {
        _logger = logger;
    }

    public unsafe void Encode(Image<Rgb24> coverImage, byte[] message, ushort seed)
    {
        _logger.Information(
            "Encoding message with image size: (width: {Width}, height: {Height}), and message length: {MessageLength} bytes",
            coverImage.Width, coverImage.Height, message.Length);

        coverImage.ProcessPixelRows(accessor =>
        {
            Random prng = new(seed);
            int pixelNumber = accessor.Width * accessor.Height;
            int step = pixelNumber / PermutationSize;
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

                        fixed (Rgb24* pixel = &row[x])
                        {
                            byte* pixelValues = (byte*) pixel;
                            for (int i = 0; i < 3; ++i)
                            {
                                int bit = (currentByte >> byteShift++) & 1;
                                pixelValues[i] = (byte) ((pixelValues[i] & ~pixelValueMask) | (bit << bitPosition));

                                if (byteShift == 8)
                                {
                                    if (++messagePosition >= message.Length)
                                    {
                                        return;
                                    }

                                    currentByte = message[messagePosition];
                                    byteShift = 0;
                                }
                            }
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(message), message, "Message is too long for the image");
        });
    }
}
