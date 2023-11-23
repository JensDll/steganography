using System.Net;
using Microsoft.AspNetCore.Http;

namespace AspNetShared.Middleware.DevelopmentProxy;

public class DevelopmentProxyMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.IsApi())
        {
            return next(context);
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return Task.CompletedTask;
    }
}
