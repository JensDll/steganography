using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace Steganography.Common;

public class LinearSearchContentTypeProvider : IContentTypeProvider
{
    private static readonly string[] s_values =
    [
        "html",
        "text/html",
        "js",
        "text/javascript",
        "css",
        "text/css",
        "json",
        "application/json",
        "svg",
        "image/svg+xml",
        "woff2",
        "font/woff2",
        "ico",
        "image/x-icon"
    ];

    public static readonly LinearSearchContentTypeProvider Instance = new();

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        ReadOnlySpan<char> path = subpath.AsSpan();
        int index = path.LastIndexOf('.') + 1;
        ReadOnlySpan<char> extension = path[index..];

        for (int i = 0; i < s_values.Length; i += 2)
        {
            if (extension.SequenceEqual(s_values[i]))
            {
                contentType = s_values[i + 1];
                return true;
            }
        }

        contentType = null;
        return false;
    }
}
