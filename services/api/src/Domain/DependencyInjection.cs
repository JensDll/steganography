using Domain.Interfaces;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjection
{
    public static void AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<IKeyService, KeyService>();
        services.AddSingleton<IEncodeService, EncodeService>();
        services.AddSingleton<IDecodeService, DecodeService>();
    }
}
