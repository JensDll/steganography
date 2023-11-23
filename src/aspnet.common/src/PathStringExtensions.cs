using Microsoft.AspNetCore.Http;

namespace aspnet.common.extensions;

public static class PathStringExtensions
{
    public static bool IsApi(this PathString path)
    {
        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
