#if NOT_RUNNING_IN_CONTAINER
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetShared.Middleware.DevelopmentProxy;

public static class DevelopmentProxyMiddlewareExtensions
{
    public static IServiceCollection AddDevelopmentProxyMiddleware(this IServiceCollection services)
    {
        services.AddScoped<DevelopmentProxyMiddleware>();
        return services;
    }

    public static void UseDevelopmentProxy(this WebApplication app, Uri baseUri)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

#pragma warning disable ASP0014 // Necessary for correct middleware order
        app.UseEndpoints(static _ =>
            { });
#pragma warning restore ASP0014

        app.UseMiddleware<DevelopmentProxyMiddleware>();

        app.UseSpa(spaBuilder =>
        {
            spaBuilder.UseProxyToSpaDevelopmentServer(baseUri);
        });
    }
}
#endif
