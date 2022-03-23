using ApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.Decode;

public class Request : IBindRequest
{
    public Image<Rgb24> CoverImage { get; private set; } = null!;
    public string Key { get; private set; } = null!;

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors,
        CancellationToken cancellationToken)
    {
        MyMultiPartReader reader = new(context, validationErrors);
        NextPart? nextPart = await reader.ReadNextPartAsync(cancellationToken);

        if (nextPart == null)
        {
            validationErrors.Add("Request is empty");
            return;
        }

        Image<Rgb24>? coverImage = await nextPart.ReadCoverImageAsync("coverImage", cancellationToken);

        if (coverImage == null)
        {
            return;
        }

        CoverImage = coverImage;

        nextPart = await reader.ReadNextPartAsync(cancellationToken);

        if (nextPart == null)
        {
            CoverImage.Dispose();
            validationErrors.Add("Request does not contain a key");
            return;
        }

        string? key = await nextPart.ReadTextAsync("key");

        if (key == null)
        {
            CoverImage.Dispose();
            return;
        }

        Key = key;
    }
}
