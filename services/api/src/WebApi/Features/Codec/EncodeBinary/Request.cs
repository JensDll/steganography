using System.Net;
using System.Text;
using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebApi.Extensions;

namespace WebApi.Features.Codec.EncodeBinary;

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

        CoverImage = await Image.LoadAsync<Rgb24>(section.Body);

        long? difference = context.Request.ContentLength - section.Body.Length;

        if (difference > section.Body.Length)
        {
            validationErrors.Add("Message is too large for the image");
            return;
        }

        section = await multipartReader.ReadNextSectionAsync();

        await using MemoryStream messageStream = new();

        while (section != null)
        {
            hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

            if (!hasContentDispositionHeader || contentDisposition == null ||
                !contentDisposition.HasFileContentDisposition())
            {
                validationErrors.Add("Request contains invalid data");
                return;
            }

            string fileName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);

            await messageStream.WriteAsync(BitConverter.GetBytes(fileNameBytes.Length));
            await messageStream.WriteAsync(fileNameBytes);

            // Write an empty length first until the length of the section is available
            await messageStream.WriteAsync(_emptyLength);
            await section.Body.CopyToAsync(messageStream);
            // Write the length of the section
            messageStream.Seek(-section.Body.Length - 4, SeekOrigin.Current);
            await messageStream.WriteAsync(BitConverter.GetBytes((int) section.Body.Length));
            messageStream.Seek(0, SeekOrigin.End);

            section = await multipartReader.ReadNextSectionAsync();
        }

        Message = messageStream.ToArray();
    }
}
