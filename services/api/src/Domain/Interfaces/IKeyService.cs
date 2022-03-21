using Domain.Enums;
using Microsoft.Extensions.Primitives;

namespace Domain.Interfaces;

public interface IKeyService
{
    public string GenerateKey();

    public string AddMetaData(string base64Key, MessageType messageType, ushort seed, int messageLength);

    public bool TryParse(StringSegment base64Key, out MessageType messageType, out ushort seed, out int messageLength,
        out StringSegment key);
}
