using Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Interfaces;

public interface IDecoder
{
    public byte[] Decode(Image<Rgb24> coverImage);

    public List<DecodedItem> ParseBytes(byte[] data, out bool isText);
}
