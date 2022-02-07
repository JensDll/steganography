using System.Security.Cryptography;

namespace Domain.Extensions;

public static class RandomNumberGeneratorExtension
{
    public static string GenerateKey(this RandomNumberGenerator rng)
    {
        var bytes = new byte[516];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}