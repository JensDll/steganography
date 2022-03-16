using ApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeText;

public class Request : IBindRequest
{
    public Image<Rgb24> CoverImage { get; private set; } = null!;

    public byte[] Message { get; private set; } = null!;

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors)
    {
        MultiPartReader reader = new(context, validationErrors);
        NextPart? nextPart = await reader.ReadNextPartAsync();

        if (nextPart == null)
        {
            validationErrors.Add("Request is empty");
            return;
        }

        Image<Rgb24>? coverImage = await nextPart.ReadCoverImageAsync("coverImage");

        if (coverImage == null)
        {
            return;
        }

        CoverImage = coverImage;

        await using MemoryStream messageStream = new();
        nextPart = await reader.ReadNextPartAsync();

        if (nextPart == null)
        {
            CoverImage.Dispose();
            validationErrors.Add("Request does not contain a message");
            return;
        }

        await nextPart.CopyFormDataToAsync(messageStream, "message");

        if (reader.HasError)
        {
            CoverImage.Dispose();
            return;
        }

        Message = messageStream.ToArray();
    }
}
