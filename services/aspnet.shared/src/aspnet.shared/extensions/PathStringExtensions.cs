using Microsoft.AspNetCore.Http;

namespace aspnet.shared.extensions;

public static class PathStringExtensions
{
    public static bool IsApi(this PathString path) => path.StartsWithSegments("/api");
}
