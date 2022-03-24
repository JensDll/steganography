using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Services;

public class KeyService : IKeyService
{
    public string ToBase64(MessageType messageType, ushort seed, int messageLength, ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> iV)
    {
        Span<byte> base64Key = stackalloc byte[51];

        base64Key[0] = (byte) messageType;
        base64Key[1] = (byte) seed;
        base64Key[2] = (byte) (seed >> 8);
        base64Key[3] = (byte) messageLength;
        base64Key[4] = (byte) (messageLength >> 8);
        base64Key[5] = (byte) (messageLength >> 16);
        base64Key[6] = (byte) (messageLength >> 24);
        key.CopyTo(base64Key[7..]);
        iV.CopyTo(base64Key[39..]);

        return Convert.ToBase64String(base64Key);
    }

    public bool TryParse(string base64Key, out MessageType messageType, out ushort seed, out int messageLength,
        out byte[] key, out byte[] iV)
    {
        messageType = MessageType.Text;
        seed = 0;
        messageLength = 0;
        key = Array.Empty<byte>();
        iV = Array.Empty<byte>();

        // The full key has a length of 51 bytes.
        // The first 7 bytes are metadata and the last 44 bytes are the key + initialization value.
        // 3 bytes result in 4 base64 characters: (51 / 3) * 4 = 17 * 4 = 68
        if (base64Key.Length != 68)
        {
            return false;
        }

        ReadOnlySpan<byte> fullKey = Convert.FromBase64String(base64Key);

        messageType = (MessageType) fullKey[0];
        seed = BitConverter.ToUInt16(fullKey[1..3]);
        messageLength = BitConverter.ToInt32(fullKey[3..7]);
        key = fullKey[7..39].ToArray();
        iV = fullKey[39..].ToArray();

        return true;
    }
}
