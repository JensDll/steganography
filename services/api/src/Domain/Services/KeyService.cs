using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Services;

public class KeyService : IKeyService
{
    public string ToBase64String(MessageType messageType, int seed, int messageLength, ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> iV)
    {
        Span<byte> base64KeyBytes = stackalloc byte[54];

        // The first 2-byte are used for the message type to get to a multiple of 3
        base64KeyBytes[0] = (byte) messageType;
        BitConverter.TryWriteBytes(base64KeyBytes[2..6], seed);
        BitConverter.TryWriteBytes(base64KeyBytes[6..10], messageLength);
        key.CopyTo(base64KeyBytes[10..]);
        iV.CopyTo(base64KeyBytes[42..]);

        return Convert.ToBase64String(base64KeyBytes);
    }

    public bool TryParse(string base64Key, out MessageType messageType, out int seed, out int messageLength,
        out byte[] key, out byte[] iV)
    {
        messageType = MessageType.Text;
        seed = 0;
        messageLength = 0;
        key = Array.Empty<byte>();
        iV = Array.Empty<byte>();

        // The full key has a length of 54-byte.
        // The first 10-byte are metadata and the last 44-byte are the key + initialization value.
        // 3-byte result in 4 base64 characters: (54 / 3) * 4 = 18 * 4 = 72
        if (base64Key.Length != 72)
        {
            return false;
        }

        ReadOnlySpan<byte> fullKey = Convert.FromBase64String(base64Key);

        messageType = (MessageType) fullKey[0];

        if (!Enum.IsDefined(messageType))
        {
            return false;
        }

        seed = BitConverter.ToInt32(fullKey[2..6]);
        messageLength = BitConverter.ToInt32(fullKey[6..10]);
        key = fullKey[10..42].ToArray();
        iV = fullKey[42..].ToArray();

        return true;
    }
}
