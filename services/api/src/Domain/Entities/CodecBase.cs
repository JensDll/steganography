using System.Buffers;
using Domain.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Entities;

public abstract class CodecBase
{
    private readonly Random _prng;
    private readonly int _coverImageSize;
    private readonly int[] _startPermutation;
    private readonly int _permutationStep;

    protected readonly Image<Rgb24> CoverImage;
    protected readonly int CoverImageCapacity;

    protected byte BitPosition;
    protected byte ByteShift;
    protected byte PixelValueMask;
    protected byte PixelIdx;

    protected int[] Permutation;
    protected int PermutationCount;
    protected int PermutationIdx;

    protected readonly int StartPermutationCount;
    protected int StartPermutationIdx;

    protected CodecBase(Image<Rgb24> coverImage, ushort seed)
    {
        Random prng = new(seed);
        _prng = prng;
        _coverImageSize = coverImage.Width * coverImage.Height;
        _permutationStep = (int) (coverImage.Width * 0.7);

        (_startPermutation, StartPermutationCount) = prng.RentPermutation(0, _permutationStep - 1);
        (Permutation, PermutationCount) =
            prng.RentPermutation(_startPermutation[0], _coverImageSize - 1, _permutationStep);

        CoverImage = coverImage;
        CoverImageCapacity = _coverImageSize * 3;
        BitPosition = 0;
        ByteShift = 0;
        PixelValueMask = 1;
        PixelIdx = 0;
    }

    protected void NextPermutation()
    {
        PermutationIdx = 0;
        ArrayPool<int>.Shared.Return(Permutation);
        if (++StartPermutationIdx < StartPermutationCount)
        {
            (Permutation, PermutationCount) =
                _prng.RentPermutation(_startPermutation[StartPermutationIdx], _coverImageSize - 1,
                    _permutationStep);
        }
    }

    protected void ReturnPermutations()
    {
        ArrayPool<int>.Shared.Return(_startPermutation);
        ArrayPool<int>.Shared.Return(Permutation);
    }
}
