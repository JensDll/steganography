using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace aspnet.common.middleware.static_compressed_file;

public static class StaticCompressedFileMiddlewareExtensions
{
    public static IServiceCollection AddStaticCompressedFileMiddleware(
        this IServiceCollection services,
        StaticCompressedFileOptions options)
    {
        services.AddOptions<StaticCompressedFileOptions>().Configure(staticFileOptions =>
        {
            staticFileOptions.RequestPath = options.RequestPath;
            staticFileOptions.OnPrepareResponse = options.OnPrepareResponse;
        });

        services.AddScoped<StaticCompressedFileMiddleware>();

        return services;
    }

    public static IServiceCollection AddStaticCompressedFileMiddleware(this IServiceCollection services)
    {
        services.AddOptions<StaticCompressedFileOptions>();
        services.AddScoped<StaticCompressedFileMiddleware>();
        return services;
    }

    public static void UseStaticCompressedFiles(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<StaticCompressedFileMiddleware>();
    }
}
