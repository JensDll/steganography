using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace MiniApi;

public static class DependencyInjection
{
    public static void AddEndpoints<T>(this IServiceCollection services)
    {
        services.RegisterEndpoints<T>().RegisterValidators<T>();
    }

    private static IServiceCollection RegisterEndpoints<T>(this IServiceCollection services)
    {
        IEnumerable<(Type endpoint, Type endpointBase)> endpoints = typeof(T).Assembly.GetTypes()
            .Where(type => type.BaseType?.BaseType != null)
            .Where(type => type.BaseType!.BaseType == typeof(EndpointBase))
            .Select(endpoint => (endpoint, endpointBase: endpoint.BaseType))!;


        foreach ((Type endpoint, Type endpointBase) in endpoints)
        {
            services.AddScoped(endpointBase, endpoint);
        }

        return services;
    }

    private static IServiceCollection RegisterValidators<T>(this IServiceCollection services)
    {
        IEnumerable<(Type validator, Type validated)> validators = typeof(T).Assembly.GetTypes()
            .Where(type => type.BaseType != null)
            .Where(type => type.BaseType!.IsGenericType
                           && type.BaseType.IsAbstract
                           && type.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .Select(validator => (validator, validated: validator.BaseType!.GetGenericArguments()[0]));

        foreach ((Type validator, Type validated) in validators)
        {
            Type validatorInterface = typeof(IValidator<>).MakeGenericType(validated);
            services.AddSingleton(validatorInterface, validator);
        }

        return services;
    }
}
