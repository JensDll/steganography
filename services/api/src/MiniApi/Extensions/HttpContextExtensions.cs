using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace MiniApi;

public static class HttpContextExtensions
{
    private static string GetBoundary(this HttpContext context, int lengthLimit = 70)
    {
        MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
        string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    private static bool IsMultipartContentType(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request.ContentType)
               && context.Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasFormDataContentDisposition(
        ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    private static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }

    private static async Task ParseFormDataAsync(this HttpContext context, Action<Stream> handelSection,
        params string[] keys)
    {
        if (context.Items["validationErrors"] is not List<string> validationErrors)
        {
            throw new InvalidOperationException("Validation errors not found in HttpContext.Items");
        }

        if (!context.IsMultipartContentType())
        {
            validationErrors.Add("Content-Type must be multipart/form-data");
            return;
        }

        string boundary = context.GetBoundary();
        MultipartReader multipartReader = new(boundary, context.Request.Body);
        MultipartSection? section = await multipartReader.ReadNextSectionAsync();

        foreach (string key in keys)
        {
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
                contentDisposition.Name != key)
            {
                validationErrors.Add("Request must contain a cover image");
                return;
            }

            section.Body.Position = 0;
            handelSection(section.Body);

            await multipartReader.ReadNextSectionAsync();
        }
    }
}
