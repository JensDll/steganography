namespace Domain.Extensions;

public static class RandomExtensions
{
    public static int[] Permutation(this Random rand, int start, int count)
    {
        int[] values = Enumerable.Range(start, count).ToArray();

        for (int i = count - 1; i > 0; --i)
        {
            int j = rand.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }
}
