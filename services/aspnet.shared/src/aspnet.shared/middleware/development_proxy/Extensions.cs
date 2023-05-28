using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace aspnet.shared.middleware.development_proxy;

public static class Extensions
{
    public static IServiceCollection AddDevelopmentProxyMiddleware(this IServiceCollection services)
    {
        services.AddScoped<DevelopmentProxyMiddleware>();
        return services;
    }

    public static void UseDevelopmentProxy(this WebApplication app)
    {
        UseDevelopmentProxy(app, "http://localhost:5173");
    }

    public static void UseDevelopmentProxy(this WebApplication app, string baseUri)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

#pragma warning disable ASP0014 // Necessary for correct middleware order
        app.UseEndpoints(_ =>
            { });
#pragma warning restore ASP0014

        app.UseMiddleware<DevelopmentProxyMiddleware>();

        app.UseSpa(spaBuilder =>
        {
            spaBuilder.UseProxyToSpaDevelopmentServer(baseUri);
        });
    }
}
