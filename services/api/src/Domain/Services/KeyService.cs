using System.Security.Cryptography;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Services;

public class KeyService : IKeyService
{
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public string AddMetaData(string base64Key, MessageType messageType, ushort seed, int messageLength)
    {
        Span<byte> metadata = stackalloc byte[9];

        metadata[2] = (byte) messageType;
        metadata[3] = (byte) seed;
        metadata[4] = (byte) (seed >> 8);
        metadata[5] = (byte) messageLength;
        metadata[6] = (byte) (messageLength >> 8);
        metadata[7] = (byte) (messageLength >> 16);
        metadata[8] = (byte) (messageLength >> 24);

        return Convert.ToBase64String(metadata) + base64Key;
    }

    public (string base64Key, byte[] key, byte[] iV) GenerateKey()
    {
        Span<byte> keyAndIv = stackalloc byte[48];
        _rng.GetBytes(keyAndIv);
        return (Convert.ToBase64String(keyAndIv), keyAndIv[..32].ToArray(), keyAndIv[32..].ToArray());
    }

    public bool TryParse(string base64Key, out MessageType messageType, out ushort seed, out int messageLength,
        out byte[] key, out byte[] iV)
    {
        key = Array.Empty<byte>();
        iV = Array.Empty<byte>();
        messageType = MessageType.Text;
        seed = 0;
        messageLength = 0;

        // The full key has a length of 57 bytes.
        // The first 9 bytes are metadata and the last 48 bytes are the key.
        // 3 bytes result in 4 base64 characters: (57 / 3) * 4 = 19 * 4 = 76
        if (base64Key.Length != 76)
        {
            return false;
        }

        ReadOnlySpan<byte> fullKey = Convert.FromBase64String(base64Key);

        messageType = (MessageType) fullKey[2];
        seed = BitConverter.ToUInt16(fullKey[3..5]);
        messageLength = BitConverter.ToInt32(fullKey[5..9]);
        key = fullKey[9..41].ToArray();
        iV = fullKey[41..].ToArray();

        return true;
    }
}
