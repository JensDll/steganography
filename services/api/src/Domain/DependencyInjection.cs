using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

public static class DependencyInjection
{
    public static void AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<IKeyGenerator, KeyGenerator>();
        services.AddSingleton<IEncoder, Encoder>();
        services.AddSingleton<IDecoder, Decoder>();
        services.AddScoped(_ => new RequestProgress());
    }
}
