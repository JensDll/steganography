using System.Buffers;

namespace Domain;

public static class RandomExtensions
{
    public static (int[] permutation, int count) RentPermutation(this Random prng, int start, int end, int step = 1)
    {
        if (start > end)
        {
            throw new ArgumentException("Start cannot be greater than end", nameof(start));
        }

        int count = (end - start) / step + 1;
        int[] values = ArrayPool<int>.Shared.Rent(count);

        for (int i = start, j = 0; i <= end; i += step, ++j)
        {
            values[j] = i;
        }

        for (int i = count - 1; i > 0; --i)
        {
            int j = prng.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return (values, count);
    }
}
