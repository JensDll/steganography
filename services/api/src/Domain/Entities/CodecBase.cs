using System.Buffers;
using Domain.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public abstract class CodecBase : IDisposable
{
    private readonly Random _prng;
    private readonly int[] _startPermutation;
    private readonly int _permutationStep;
    private readonly int _permutationEnd;

    protected readonly Image<Rgb24> CoverImage;
    protected readonly int CoverImageCapacity;

    protected byte BitPosition;
    protected byte ByteShift;
    protected byte PixelValueMask = 0b1111_1110;
    protected byte PixelIdx;

    protected int[] Permutation;
    protected int PermutationCount;
    protected int PermutationIdx;

    protected readonly int StartPermutationCount;
    protected int StartPermutationIdx = 1;

    protected CodecBase(Image<Rgb24> coverImage, int seed)
    {
        int coverImageSize = coverImage.Width * coverImage.Height;
        _prng = new Random(seed);
        _permutationEnd = coverImageSize - 1;
        _permutationStep = (int) (coverImage.Width * 0.7);

        (_startPermutation, StartPermutationCount) = _prng.RentPermutation(0, _permutationStep - 1);
        (Permutation, PermutationCount) =
            _prng.RentPermutation(_startPermutation[0], _permutationEnd, _permutationStep);

        CoverImage = coverImage;
        CoverImageCapacity = coverImageSize * 3;
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
            ++BitPosition;
            PixelValueMask = (byte) ~(~PixelValueMask << 1);
        }

        PermutationIdx = 0;
        ArrayPool<int>.Shared.Return(Permutation);
        (Permutation, PermutationCount) =
            _prng.RentPermutation(_startPermutation[StartPermutationIdx++], _permutationEnd, _permutationStep);
    }
}
