namespace Domain.Entities;

public class DecodedItem
{
    public string Name { get; init; } = string.Empty;

    public byte[] Data { get; init; } = Array.Empty<byte>();
}
