using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Services;

public class KeyService : IKeyService
{
    public string ToBase64(MessageType messageType, int seed, int messageLength, ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> iV)
    {
        Span<byte> base64Key = stackalloc byte[54];

        base64Key[0] = (byte) messageType;
        base64Key[2] = (byte) seed;
        base64Key[3] = (byte) (seed >> 8);
        base64Key[4] = (byte) (seed >> 16);
        base64Key[5] = (byte) (seed >> 24);
        base64Key[6] = (byte) messageLength;
        base64Key[7] = (byte) (messageLength >> 8);
        base64Key[8] = (byte) (messageLength >> 16);
        base64Key[9] = (byte) (messageLength >> 24);
        key.CopyTo(base64Key[10..]);
        iV.CopyTo(base64Key[42..]);

        return Convert.ToBase64String(base64Key);
    }

    public bool TryParse(string base64Key, out MessageType messageType, out int seed, out int messageLength,
        out byte[] key, out byte[] iV)
    {
        messageType = MessageType.Text;
        seed = 0;
        messageLength = 0;
        key = Array.Empty<byte>();
        iV = Array.Empty<byte>();

        // The full key has a length of 54 bytes.
        // The first 10 bytes are metadata and the last 44 bytes are the key + initialization value.
        // 3 bytes result in 4 base64 characters: (54 / 3) * 4 = 18 * 4 = 72
        if (base64Key.Length != 72)
        {
            return false;
        }

        ReadOnlySpan<byte> fullKey = Convert.FromBase64String(base64Key);

        if (fullKey[0] > 1)
        {
            return false;
        }

        messageType = (MessageType) fullKey[0];
        seed = BitConverter.ToInt32(fullKey[2..6]);
        messageLength = BitConverter.ToInt32(fullKey[6..10]);
        key = fullKey[10..42].ToArray();
        iV = fullKey[42..].ToArray();

        return true;
    }
}
