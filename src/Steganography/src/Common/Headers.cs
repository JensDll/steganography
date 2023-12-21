using Microsoft.Extensions.Primitives;

namespace Steganography.Common;

internal static class Headers
{
    public static readonly StringValues NoSniff = new("nosniff");

    public static readonly StringValues CacheControl = new("public,max-age=31536000,immutable");

    public static readonly StringValues CacheControlHtml = new("private,no-store,no-cache,max-age=0,must-revalidate");

    public static readonly StringValues XXSSProtection = new("0");
}
