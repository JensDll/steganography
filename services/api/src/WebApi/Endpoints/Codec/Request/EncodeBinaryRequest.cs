using System.Net;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.Endpoints.Codec.Request;

public class EncodeBinaryRequest : MiniApi.Request
{
    public Image<Rgb24> CoverImage { get; set; } = null!;

    public byte[] Data { get; set; } = null!;

    public override async Task BindAsync(HttpContext context, List<string> validationErrors)
    {
        if (!IsMultipartContentType(context))
        {
            validationErrors.Add("Content-Type must be multipart/form-data");
            return;
        }

        string boundary = GetBoundary(context);
        MultipartReader multipartReader = new(boundary, context.Request.Body);
        MultipartSection? section = await multipartReader.ReadNextSectionAsync();

        if (section == null)
        {
            validationErrors.Add("Request is empty");
            return;
        }

        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition);

        if (!hasContentDispositionHeader || contentDisposition == null ||
            !HasFileContentDisposition(contentDisposition) ||
            contentDisposition.Name != "coverImage")
        {
            validationErrors.Add("Request must contain a cover image");
            return;
        }

        section.Body.Position = 0;

        CoverImage = await Image.LoadAsync<Rgb24>(section.Body);

        long? difference = context.Request.ContentLength - section.Body.Length;

        if (difference >= section.Body.Length)
        {
            validationErrors.Add("Message is too large for the image");
            return;
        }

        section = await multipartReader.ReadNextSectionAsync();

        await using MemoryStream data = new();

        while (section != null)
        {
            hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

            if (!hasContentDispositionHeader || contentDisposition == null ||
                !HasFileContentDisposition(contentDisposition))
            {
                validationErrors.Add("Request contains invalid data");
                return;
            }

            string fileName = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);

            await data.WriteAsync(BitConverter.GetBytes(fileNameBytes.Length));
            await data.WriteAsync(fileNameBytes);

            await using MemoryStream bodyContent = new();
            await section.Body.CopyToAsync(bodyContent);

            await data.WriteAsync(BitConverter.GetBytes((int) section.Body.Length));
            bodyContent.Seek(0, SeekOrigin.Begin);
            await bodyContent.CopyToAsync(data);

            section = await multipartReader.ReadNextSectionAsync();
        }

        Data = data.ToArray();
    }
}
