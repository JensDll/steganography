using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services.AddSingleton<IKeyService, KeyService>();
    }
}
