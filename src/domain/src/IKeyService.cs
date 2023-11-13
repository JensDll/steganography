namespace domain;

public interface IKeyService
{
    public string ToBase64String(MessageType messageType, int seed, int messageLength, ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> iV);

    public bool TryParse(string base64Key, out MessageType messageType, out int seed, out int messageLength,
        out byte[] key, out byte[] iV);
}
