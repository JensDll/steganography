using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApi.Extensions;

public static class ModelBindingExtensions
{
    public static MultipartReader GetMultipartReader(this HttpContext context)
    {
        if (!context.IsMultipartContentType())
        {
            throw new ModelBindingException("Content-Type must be multipart/form-data");
        }

        string boundary = context.GetBoundary();

        return new MultipartReader(boundary, context.Request.Body);
    }

    public static async Task<Image<Rgb24>> GetCoverImageAsync(this MultipartReader reader, HttpContext context)
    {
        MultipartSection? section = await reader.ReadNextSectionAsync();

        if (section == null)
        {
            throw new ModelBindingException("Request is empty");
        }

        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition,
                out ContentDispositionHeaderValue? contentDisposition);

        if (!hasContentDispositionHeader || contentDisposition == null ||
            !HasFileContentDisposition(contentDisposition) ||
            contentDisposition.Name != "coverImage")
        {
            throw new ModelBindingException("Request must contain a cover image");
        }

        section.Body.Position = 0;

        Image<Rgb24>? image = await Image.LoadAsync<Rgb24>(section.Body);

        long? difference = context.Request.ContentLength - section.Body.Length;

        if (difference >= section.Body.Length)
        {
            throw new ModelBindingException("Message is too large for the image");
        }

        return image;
    }

    public static string GetBoundary(this HttpContext context, int lengthLimit = 70)
    {
        MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
        string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new ModelBindingException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new ModelBindingException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request.ContentType)
               && context.Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasFormDataContentDisposition(
        this ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    public static bool HasFileContentDisposition(this ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}
