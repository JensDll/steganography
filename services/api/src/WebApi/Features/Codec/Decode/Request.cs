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
        MultipartReader multipartReader;

        try
        {
            multipartReader = new MultipartReader(context);
        }
        catch
        {
            return null;
        }

        NextSection? nextSection = await multipartReader.ReadNextSectionAsync();
        FileMultipartSection? fileSection = nextSection?.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return null;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync<PngFormat>();

        if (coverImage is null)
        {
            return null;
        }

        context.Response.RegisterForDispose(coverImage);

        nextSection = await multipartReader.ReadNextSectionAsync();
        FormMultipartSection? formSection = nextSection?.AsFormSection("key");

        if (formSection is null)
        {
            return null;
        }

        string key = await formSection.GetValueAsync();

        return new DecodeRequest()
        {
            CoverImage = coverImage,
            CoverImageCapacity = coverImage.Width * coverImage.Height * 3,
            Key = key
        };
    }
}
