namespace Domain.Entities;

public static class ParsingUtils
{
    public static void CopyAsInt32(ReadOnlySpan<byte> digits, Span<byte> destination)
    {
        int result = DigitsToInt32(digits);
        destination[0] = (byte) result;
        destination[1] = (byte) (result >> 8);
        destination[2] = (byte) (result >> 16);
        destination[3] = (byte) (result >> 24);
    }

    public static int DigitsToInt32(ReadOnlySpan<byte> digits)
    {
        int result = 0;
        int placeValue = 1;

        for (int i = digits.Length - 1; i >= 0; --i, placeValue *= 10)
        {
            result += (digits[i] - 48) * placeValue;
        }

        return result;
    }
}
