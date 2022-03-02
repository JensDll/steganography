using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Domain.Interfaces;

public interface IEncoder
{
    public void Encode(Image<Rgb24> coverImage, byte[] data);
}
