using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace steganography.api.extensions;

public static class FileMultipartSectionExtensions
{
    public static async Task<Image<Rgb24>?> ReadCoverImageAsync(this FileMultipartSection fileSection,
        CancellationToken cancellationToken)
    {
        try
        {
            return await Image.LoadAsync<Rgb24>(fileSection.FileStream!, cancellationToken);
        }
        catch (UnknownImageFormatException)
        { }

        return null;
    }
}
