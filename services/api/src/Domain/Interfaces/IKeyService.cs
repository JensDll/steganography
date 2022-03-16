namespace Domain.Interfaces;

public interface IKeyService
{
    public string Generate(int keyLength);

    public string AddMetaData(string base64Key, ushort seed, int messageLength);

    public bool TryParse(string base64Key, out ushort seed, out int messageLength, out string key);
}
