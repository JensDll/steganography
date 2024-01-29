#if NOT_RUNNING_IN_CONTAINER
namespace Steganography.Common;

internal static class DevelopmentProxyMiddlewareExtensions
{
    public static IServiceCollection AddDevelopmentProxyMiddleware(this IServiceCollection services)
    {
        services.AddSingleton<DevelopmentProxyMiddleware>();
        return services;
    }

    public static void UseDevelopmentProxy(this WebApplication app, string baseUri)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.UseEndpoints(_ =>
            { });

        app.UseMiddleware<DevelopmentProxyMiddleware>();

        app.UseSpa(spaBuilder =>
        {
            spaBuilder.UseProxyToSpaDevelopmentServer(baseUri);
        });
    }
}
#endif
