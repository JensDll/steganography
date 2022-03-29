using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.ModelBinding;

public class MyFileMultipartSection : MyMultipartSection
{
    public MyFileMultipartSection(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
        List<string> validationErrors) : base(section, contentDisposition, validationErrors)
    {
        FileName = HeaderUtilities.RemoveQuotes(
            contentDisposition.FileNameStar.HasValue
                ? contentDisposition.FileNameStar
                : contentDisposition.FileName).ToString();
    }

    public string FileName { get; }

    public async Task<Image<Rgb24>?> ReadCoverImageAsync(CancellationToken cancellationToken = default)
    {
        Image<Rgb24>? coverImage = null;

        try
        {
            coverImage = await Image.LoadAsync<Rgb24>(Body, cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            ValidationErrors.Add("Unsupported cover image format");
        }

        return coverImage;
    }
}
