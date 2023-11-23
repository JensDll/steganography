using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace AspNetShared.Middleware.StaticCompressedFile;

internal static class Helper
{
    private static readonly Dictionary<StringSegment, int> s_contentEncodingOrder = new()
    {
        ["br"] = 1,
        ["gzip"] = 0
    };

    public static bool TryGetContentEncoding(RequestHeaders requestHeaders, out StringSegment contentEncoding)
    {
        int best = -1;

        contentEncoding = StringSegment.Empty;

        foreach (StringWithQualityHeaderValue value in requestHeaders.AcceptEncoding)
        {
            if (!s_contentEncodingOrder.TryGetValue(value.Value, out int order))
            {
                continue;
            }

            if (order > best)
            {
                contentEncoding = value.Value;
                best = order;
            }
        }

        return best > -1;
    }

    internal static bool PathEndsInSlash(PathString path)
    {
        return path.HasValue && path.Value!.EndsWith('/');
    }

    internal static bool TryMatchPath(PathString path, PathString other, out PathString subPath)
    {
        return path.StartsWithSegments(other, out subPath);
    }
}
