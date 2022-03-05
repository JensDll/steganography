namespace Domain.Interfaces;

public interface IKeyGenerator
{
    public string GenerateKey(int keyLength);

    public string AddMetaData(string base64Key, ushort seed, int messageLength);

    public bool TryParseKey(string base64Key, out ushort seed, out int messageLength, out string key);
}
