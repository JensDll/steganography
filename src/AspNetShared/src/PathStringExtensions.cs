using Microsoft.AspNetCore.Http;

namespace AspNetShared;

public static class PathStringExtensions
{
    public static bool IsApi(this PathString path)
    {
        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
