using Microsoft.Extensions.DependencyInjection;

namespace steganography.domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services.AddSingleton<IKeyService, KeyService>();
    }
}
