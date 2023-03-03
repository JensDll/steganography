using Microsoft.AspNetCore.WebUtilities;
using MinimalApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.Extensions;
using MultipartReader = MinimalApiBuilder.MultipartReader;

namespace WebApi.Features.Codec.Decode;

public class DecodeRequest
{
    public required Image<Rgb24> CoverImage { get; init; }
    public required int CoverImageCapacity { get; init; }
    public required string Key { get; init; }

    public static async ValueTask<DecodeRequest?> BindAsync(HttpContext context)
    {
        MultipartReader? multipartReader = MultipartReader.Create(context);

        if (multipartReader is null)
        {
            return null;
        }

        CancellationToken cancellationToken = context.RequestAborted;

        NextSection? nextSection = await multipartReader.ReadNextSectionAsync(cancellationToken);
        FileMultipartSection? fileSection = nextSection?.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return null;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync(cancellationToken);

        if (coverImage?.Metadata.DecodedImageFormat is not PngFormat)
        {
            return null;
        }

        context.Response.RegisterForDispose(coverImage);

        nextSection = await multipartReader.ReadNextSectionAsync(cancellationToken);
        FormMultipartSection? formSection = nextSection?.AsFormSection("key");

        if (formSection is null)
        {
            return null;
        }

        string key = await formSection.GetValueAsync(cancellationToken);

        return new DecodeRequest()
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            Key = key
        };
    }
}
