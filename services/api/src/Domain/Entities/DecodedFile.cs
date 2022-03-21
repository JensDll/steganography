namespace Domain.Entities;

public class DecodedFile
{
    public string Name { get; init; } = null!;

    public ReadOnlyMemory<byte> Data { get; init; }
}
