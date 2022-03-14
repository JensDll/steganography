using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.Extensions;

namespace WebApi.Features.Codec.EncodeText;

public class Request : IBindRequest
{
    private static readonly byte[] _emptyLength = new byte[4];

    public Image<Rgb24> CoverImage { get; set; } = null!;

    public byte[] Message { get; set; } = null!;

    public async ValueTask BindAsync(HttpContext context, List<string> validationErrors)
    {
        if (!context.IsMultipartContentType())
        {
            validationErrors.Add("Content-Type must be multipart/form-data");
            return;
        }

        string boundary = context.GetBoundary();
        MultipartReader multipartReader = new(boundary, context.Request.Body);
        MultipartSection? section = await multipartReader.ReadNextSectionAsync();

        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(
                section?.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition);

        if (!hasContentDispositionHeader || contentDisposition == null ||
            !contentDisposition.HasFileContentDisposition() ||
            contentDisposition.Name != "coverImage")
        {
            validationErrors.Add("Request must contain a cover image");
            return;
        }

        section!.Body.Position = 0;

        try
        {
            CoverImage = await Image.LoadAsync<Rgb24>(section.Body);
        }
        catch (UnknownImageFormatException)
        {
            validationErrors.Add("Unsupported image format");
            return;
        }

        long? difference = context.Request.ContentLength - section.Body.Length;

        if (difference > section.Body.Length)
        {
            validationErrors.Add("Message is too large for the image");
            return;
        }

        section = await multipartReader.ReadNextSectionAsync();

        hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(section?.ContentDisposition, out contentDisposition);

        if (!hasContentDispositionHeader || contentDisposition == null ||
            !contentDisposition.HasFormDataContentDisposition() ||
            contentDisposition.Name != "message")
        {
            validationErrors.Add("Request must contain a message");
            return;
        }

        await using MemoryStream messageStream = new();
        await messageStream.WriteAsync(_emptyLength);
        await section!.Body.CopyToAsync(messageStream);

        Message = messageStream.ToArray();
    }
}
