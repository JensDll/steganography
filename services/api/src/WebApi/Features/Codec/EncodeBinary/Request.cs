using ApiBuilder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.ModelBinding;

namespace WebApi.Features.Codec.EncodeBinary;

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
            validationErrors.Add("Request does not contain a cover image");
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

        int i = 0;
        while (nextPart != null)
        {
            await nextPart.CopyFileToAsync(messageStream, i++.ToString());

            if (reader.HasError)
            {
                CoverImage.Dispose();
                return;
            }

            nextPart = await reader.ReadNextPartAsync();
        }

        Message = messageStream.ToArray();
    }
}
