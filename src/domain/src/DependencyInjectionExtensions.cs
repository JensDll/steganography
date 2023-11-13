using Microsoft.Extensions.DependencyInjection;

namespace steganography.domain;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services.AddSingleton<IKeyService, KeyService>();
    }
}
