﻿using System.Diagnostics.CodeAnalysis;
using ApiBuilder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApi.ModelBinding;

public static class ModelBindingExtensions
{
    public static string GetBoundary(this HttpContext context, int lengthLimit = 70)
    {
        if (context.Request.ContentType == null)
        {
            throw new ModelBindingException("Missing content-type header");
        }

        MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
        string? boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

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

    public static bool IsMultipart(this HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Request.ContentType) &&
               context.Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFormData(this MultipartSection section,
        [NotNullWhen(true)] out ContentDispositionHeaderValue? contentDisposition)
    {
        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        return hasContentDispositionHeader &&
               contentDisposition is not null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               string.IsNullOrEmpty(contentDisposition.FileName.Value) &&
               string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    public static bool IsFile(this MultipartSection section,
        [NotNullWhen(true)] out ContentDispositionHeaderValue? contentDisposition)
    {
        bool hasContentDispositionHeader =
            ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

        return hasContentDispositionHeader &&
               contentDisposition is not null &&
               contentDisposition.DispositionType.Equals("form-data") &&
               (!string.IsNullOrEmpty(contentDisposition.FileName.Value) ||
                !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}
