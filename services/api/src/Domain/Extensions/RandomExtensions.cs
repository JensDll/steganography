namespace Domain.Extensions;

public static class RandomExtensions
{
    public static int[] Permutation(this Random prng, int start, int end, int step)
    {
        if (start > end)
        {
            throw new ArgumentException("Start cannot be greater than end");
        }

        int[] values = new int[(end - start) / step + 1];

        for (int i = start, j = 0; i <= end; i += step, ++j)
        {
            values[j] = i;
        }

        for (int i = values.Length - 1; i > 0; --i)
        {
            int j = prng.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }
}
