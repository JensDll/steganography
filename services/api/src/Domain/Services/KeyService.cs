using System.Security.Cryptography;
using Domain.Interfaces;

namespace Domain.Services;

public class KeyService : IKeyService
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string Generate(int keyLength)
    {
        byte[] bytes = new byte[keyLength / 4 * 3];
        _rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string AddMetaData(string base64Key, ushort seed, int messageLength)
    {
        byte[] bytes = new byte[sizeof(ushort) + sizeof(int)];

        BitConverter.GetBytes(seed).CopyTo(bytes, 0);
        BitConverter.GetBytes(messageLength).CopyTo(bytes, sizeof(ushort));

        return Convert.ToBase64String(bytes) + base64Key;
    }

    public bool TryParse(string base64Key, out ushort seed, out int messageLength, out string key)
    {
        seed = 0;
        messageLength = 0;
        key = "";

        if (base64Key.Length <= 8)
        {
            return false;
        }

        key = base64Key[8..];

        Span<byte> bytes = new(Convert.FromBase64String(base64Key[..8]));

        seed = BitConverter.ToUInt16(bytes[..2]);
        messageLength = BitConverter.ToInt32(bytes[2..]);

        return true;
    }
}
