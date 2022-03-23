using Domain.Enums;

namespace Domain.Interfaces;

public interface IKeyService
{
    public (string base64Key, byte[] key, byte[] iV) GenerateKey();

    public string AddMetaData(string base64Key, MessageType messageType, ushort seed, int messageLength);

    public bool TryParse(string base64Key, out MessageType messageType, out ushort seed, out int messageLength,
        out byte[] key, out byte[] iV);
}
