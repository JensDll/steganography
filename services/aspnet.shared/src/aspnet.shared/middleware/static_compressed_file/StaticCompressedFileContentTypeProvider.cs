using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.StaticFiles;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileContentTypeProvider : IContentTypeProvider
{
    private static readonly IReadOnlyDictionary<string, string> s_mappings = new Dictionary<string, string>
    {
        { ".css", "text/css" },
        { ".js", "application/javascript" }
    };

    public static StaticCompressedFileContentTypeProvider Instance { get; } = new();

    public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
    {
        string? extension = GetExtension(subpath);

        if (extension is not null)
        {
            return s_mappings.TryGetValue(extension, out contentType);
        }

        contentType = null;
        return false;
    }

    private static string? GetExtension(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        int index = path.LastIndexOf('.');

        return index < 0 ? null : path[index..];
    }
}
