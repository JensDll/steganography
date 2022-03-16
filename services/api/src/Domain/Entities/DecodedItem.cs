namespace Domain.Entities;

public class DecodedItem
{
    public string Name { get; init; } = null!;

    public ReadOnlyMemory<byte> Data { get; init; }
}
