namespace Domain.Entities;

public class DecodedItem
{
    public string Name { get; init; } = null!;

    public byte[] Data { get; init; } = null!;
}
