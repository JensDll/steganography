using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApi.ModelBinding;

public static class ModelBindingExtensions
{
    public static string GetBoundary(this HttpContext context, int lengthLimit = 70)
    {
        MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
        string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new ModelBindingException("Missing content-type boundary");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new ModelBindingException(
                $"Multipart boundary length limit {lengthLimit} exceeded");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request.ContentType) &&
               context.Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasFormDataContentDisposition(this MultipartSection section, string sectionName,
        out ContentDispositionHeaderValue? contentDisposition)
    {
        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        return hasContentDispositionHeader &&
               contentDisposition != null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
               string.IsNullOrEmpty(contentDisposition.FileNameStar.Value) &&
               contentDisposition.Name.Equals(sectionName);
    }

    public static bool HasFileContentDisposition(this MultipartSection section, string sectionName,
        out ContentDispositionHeaderValue? contentDisposition)
    {
        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        return hasContentDispositionHeader &&
               contentDisposition != null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)) &&
               contentDisposition.Name.Equals(sectionName);
    }
}
