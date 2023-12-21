using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace Steganography.Common;

public class ContentTypeProvider : IContentTypeProvider
{
    private static readonly string[] s_values =
    [
        "html",
        "text/html",
        "js",
        "application/javascript",
        "css",
        "text/css",
        "json",
        "application/json",
        "svg",
        "image/svg+xml"
    ];

    public static ContentTypeProvider Instance { get; } = new();

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        contentType = null;

        ReadOnlySpan<char> path = subpath.AsSpan();
        int index = path.LastIndexOf('.') + 1;

        if (index == 0)
        {
            return false;
        }

        ReadOnlySpan<char> extension = path[index..];

        for (int i = 0; i < s_values.Length; i += 2)
        {
            if (extension.SequenceEqual(s_values[i]))
            {
                contentType = s_values[i + 1];
                return true;
            }
        }

        return false;
    }
}
