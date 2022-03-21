using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Interfaces;

public interface IDecodeService
{
    public byte[] Decode(Image<Rgb24> coverImage, ushort seed, int messageLength);

    public IEnumerable<DecodedFile> ParseFiles(ReadOnlyMemory<byte> message);
}
