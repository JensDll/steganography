using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Interfaces;

public interface IEncodeService
{
    public void Encode(Image<Rgb24> coverImage, byte[] message, ushort seed);
}
