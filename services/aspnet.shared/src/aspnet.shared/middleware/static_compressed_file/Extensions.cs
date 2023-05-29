using aspnet.shared.options.fluent_validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace aspnet.shared.middleware.static_compressed_file;

public static class Extensions
{
    public static IServiceCollection AddStaticCompressedFileMiddleware(
        this IServiceCollection services,
        string requestPath)
    {
        StaticCompressedFileOptions options = new()
        {
            RequestPath = requestPath
        };

        AddStaticCompressedFileMiddleware(services, options);

        return services;
    }

    public static IServiceCollection AddStaticCompressedFileMiddleware(
        this IServiceCollection services,
        StaticCompressedFileOptions options)
    {
        services.AddOptions<StaticCompressedFileOptions>().Configure(o =>
        {
            o.RequestPath = options.RequestPath;
        }).Validate();

        services.AddScoped<StaticCompressedFileMiddleware>();

        return services;
    }

    public static IServiceCollection AddStaticCompressedFileMiddleware(this IServiceCollection services)
    {
        services.AddOptions<StaticCompressedFileOptions>().Validate();
        services.AddScoped<StaticCompressedFileMiddleware>();
        return services;
    }

    public static void UseStaticCompressedFiles(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<StaticCompressedFileMiddleware>();
    }

    private static void Validate(this OptionsBuilder<StaticCompressedFileOptions> builder)
    {
        builder.ValidateFluentValidation<StaticCompressedFileOptions, StaticCompressedFileOptionsValidator>()
            .ValidateOnStart();
    }
}
