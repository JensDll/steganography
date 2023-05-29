using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace aspnet.common.middleware.static_compressed_file;

internal static class Helper
{
    private static readonly IReadOnlyDictionary<StringSegment, int> s_contentEncodingOrder =
        new Dictionary<StringSegment, int>
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

            contentEncoding = value.Value;

            if (order > best)
            {
                best = order;
            }
        }

        return best > -1;
    }

    internal static bool PathEndsInSlash(PathString path)
    {
        return path.HasValue && path.Value!.EndsWith("/", StringComparison.Ordinal);
    }

    internal static bool TryMatchPath(PathString path, PathString other, out PathString subPath)
    {
        return path.StartsWithSegments(other, out subPath);
    }
}
