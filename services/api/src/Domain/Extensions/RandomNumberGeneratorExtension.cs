using System.Security.Cryptography;

namespace Domain.Extensions;

public static class RandomNumberGeneratorExtension
{
    public static string GenerateKey(this RandomNumberGenerator rng, int length)
    {
        byte[] bytes = new byte[length / 4 * 3];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
