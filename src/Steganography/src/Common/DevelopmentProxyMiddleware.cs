#if NOT_RUNNING_IN_CONTAINER
namespace Steganography.Common;

internal sealed class DevelopmentProxyMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.Ordinal))
        {
            return next(context);
        }

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }
}
#endif
