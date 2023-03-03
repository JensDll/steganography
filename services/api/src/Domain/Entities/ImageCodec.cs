using System.Buffers;
using Domain.Extensions;

namespace Domain.Entities;

public abstract class ImageCodec : IDisposable
{
    private readonly Random _prng;
    private readonly int[] _startPermutation;
    private readonly int _permutationStep;
    private readonly int _permutationEnd;

    protected readonly Image<Rgb24> CoverImage;

    protected byte BitPosition;
    protected byte ByteShift;
    protected byte PixelValueMask = 0b1111_1110;
    protected byte PixelIdx;

    protected int[] Permutation;
    protected int PermutationCount;
    protected int PermutationIdx;

    protected readonly int StartPermutationCount;
    protected int StartPermutationIdx = 1;

    protected ImageCodec(Image<Rgb24> coverImage, int seed)
    {
        int coverImageSize = coverImage.Width * coverImage.Height;
        _prng = new Random(seed);
        _permutationEnd = coverImageSize - 1;
        _permutationStep = (int)(coverImage.Width * 0.7);

        (_startPermutation, StartPermutationCount) = _prng.RentPermutation(0, _permutationStep - 1);
        (Permutation, PermutationCount) =
            _prng.RentPermutation(_startPermutation[0], _permutationEnd, _permutationStep);

        CoverImage = coverImage;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ArrayPool<int>.Shared.Return(_startPermutation);
        ArrayPool<int>.Shared.Return(Permutation);
    }

    protected void NextPermutation()
    {
        if (StartPermutationIdx == StartPermutationCount)
        {
            StartPermutationIdx = 0;

            if (++BitPosition == 8)
            {
                throw new InvalidOperationException("The message is too long for the cover image");
            }

            PixelValueMask = (byte)~(~PixelValueMask << 1);
        }

        PermutationIdx = 0;
        ArrayPool<int>.Shared.Return(Permutation);
        (Permutation, PermutationCount) =
            _prng.RentPermutation(_startPermutation[StartPermutationIdx++], _permutationEnd, _permutationStep);
    }
}
