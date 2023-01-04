using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.Extensions;

public static class FileMultipartSectionExtensions
{
    public static async Task<Image<Rgb24>?> ReadCoverImageAsync(this FileMultipartSection fileSection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Image.LoadAsync<Rgb24>(fileSection.FileStream, cancellationToken);
        }
        catch (UnknownImageFormatException) { }

        return null;
    }

    public static async Task<Image<Rgb24>?> ReadCoverImageAsync<TImageFormat>(this FileMultipartSection fileSection,
        CancellationToken cancellationToken = default)
        where TImageFormat : IImageFormat
    {
        try
        {
            (Image<Rgb24> coverImage, IImageFormat? format) =
                await Image.LoadWithFormatAsync<Rgb24>(fileSection.FileStream, cancellationToken);

            if (format is TImageFormat)
            {
                return coverImage;
            }
        }
        catch (UnknownImageFormatException) { }

        return null;
    }
}
