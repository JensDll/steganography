using ApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.Decode;

public class Request : IBindRequest, IDisposable
{
    public Image<Rgb24> CoverImage { get; private set; } = null!;

    public int CoverImageCapacity { get; private set; }

    public string Key { get; private set; } = null!;

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors,
        CancellationToken cancellationToken)
    {
        MyMultiPartReader reader = new(context, validationErrors);
        NextSection? nextSection = await reader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            validationErrors.Add("The request does not contain a cover image");
            return;
        }

        MyFileMultipartSection? fileSection = nextSection.AsFileSection("coverImage");

        if (fileSection is null)
        {
            return;
        }

        Image<Rgb24>? coverImage = await fileSection.ReadCoverImageAsync<PngFormat>(cancellationToken);

        if (coverImage is null)
        {
            return;
        }

        CoverImage = coverImage;
        CoverImageCapacity = CoverImage.Width * CoverImage.Height * 3;

        nextSection = await reader.ReadNextSectionAsync(cancellationToken);

        if (nextSection is null)
        {
            validationErrors.Add("The request does not contain a key");
            return;
        }

        MyFormMultipartSection? formSection = nextSection.AsFormSection("key");

        if (formSection is null)
        {
            return;
        }

        Key = await formSection.GetValueAsync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // ReSharper disable once ConstantConditionalAccessQualifier
        CoverImage?.Dispose();
    }
}
