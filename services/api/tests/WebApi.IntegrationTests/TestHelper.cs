using System.Text;

namespace WebApi.IntegrationTests;

public static class TestHelper
{
    public static string AlterBase64Key(string key)
    {
        StringBuilder keyBuilder = new(key);
        int lastIdx = Array.IndexOf(Base64Chars, keyBuilder[^1]);

        if (lastIdx == Base64Chars.Length - 1)
        {
            keyBuilder[^1] = Base64Chars[0];
        }
        else
        {
            keyBuilder[^1] = Base64Chars[lastIdx + 1];
        }

        return keyBuilder.ToString();
    }

    private static readonly char[] Base64Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();
}
