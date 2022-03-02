using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MiniApi;

public class Request
{
    protected static string GetBoundary(HttpContext context, int lengthLimit = 70)
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

    protected static bool IsMultipartContentType(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request.ContentType)
               && context.Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    protected static bool HasFormDataContentDisposition(
        ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    protected static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        return contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }

    public virtual Task BindAsync(HttpContext context, List<string> validationErrors)
    {
        return Task.CompletedTask;
    }
}
