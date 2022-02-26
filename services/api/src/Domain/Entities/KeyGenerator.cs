using System.Security.Cryptography;
using Domain.Interfaces;

namespace Domain.Entities;

public class KeyGenerator : IKeyGenerator
{
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string GenerateKey(int length)
    {
        byte[] bytes = new byte[length / 4 * 3];
        _rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
