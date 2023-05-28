using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace aspnet.shared.middleware.static_compressed_file;

public static class Extensions
{
    public static IServiceCollection AddStaticCompressedFileMiddleware(this IServiceCollection services)
    {
        services.AddScoped<StaticCompressedFileMiddleware>();
        return services;
    }

    public static void UseStaticCompressedFiles(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<StaticCompressedFileMiddleware>();
    }
}
