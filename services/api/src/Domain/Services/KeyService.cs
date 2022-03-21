using System.Security.Cryptography;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Primitives;

namespace Domain.Services;

public class KeyService : IKeyService
{
    public string AddMetaData(string base64Key, MessageType messageType, ushort seed, int messageLength)
    {
        byte[] metadata = new byte[9];

        metadata[2] = (byte) messageType;
        BitConverter.GetBytes(seed).CopyTo(metadata, 3);
        BitConverter.GetBytes(messageLength).CopyTo(metadata, 5);

        return Convert.ToBase64String(metadata) + base64Key;
    }

    public string GenerateKey()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] key = new byte[48];
        rng.GetBytes(key);
        return Convert.ToBase64String(key);
    }

    public bool TryParse(StringSegment base64Key, out MessageType messageType, out ushort seed, out int messageLength,
        out StringSegment key)
    {
        messageType = MessageType.Text;
        seed = 0;
        messageLength = 0;
        key = string.Empty;

        // The full key has a length of 57 bytes
        // (57 / 3) * 4 = 76 base64 characters
        if (base64Key.Length != 76)
        {
            return false;
        }

        key = base64Key.Subsegment(12);

        ReadOnlySpan<byte> metadata = Convert.FromBase64String(base64Key.Substring(0, 12));

        messageType = (MessageType) metadata[2];
        seed = BitConverter.ToUInt16(metadata[3..5]);
        messageLength = BitConverter.ToInt32(metadata[5..]);

        return true;
    }
}
