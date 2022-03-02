namespace Domain.Entities;

public class DecodedItem
{
    public string Name { get; set; } = null!;

    public byte[] Data { get; set; } = null!;
}
