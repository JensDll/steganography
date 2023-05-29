using System.Net;
using aspnet.common.extensions;
using Microsoft.AspNetCore.Http;

namespace aspnet.common.middleware.development_proxy;

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
