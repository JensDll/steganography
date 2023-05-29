using System.Text;

namespace steganography.api.tests;

public static class TestHelper
{
    public static string AlterBase64Key(string key)
    {
        StringBuilder keyBuilder = new(key);
        int lastIdx = Array.IndexOf(s_base64Chars, keyBuilder[^1]);

        if (lastIdx == s_base64Chars.Length - 1)
        {
            keyBuilder[^1] = s_base64Chars[0];
        }
        else
        {
            keyBuilder[^1] = s_base64Chars[lastIdx + 1];
        }

        return keyBuilder.ToString();
    }

    private static readonly char[] s_base64Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();
}
