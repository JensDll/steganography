using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBuilder;

public static class DependencyInjection
{
    public static void AddEndpoints<T>(this IServiceCollection services)
    {
        services.RegisterEndpoints<T>();
        services.RegisterValidators<T>();
    }

    private static IServiceCollection RegisterEndpoints<T>(this IServiceCollection services)
    {
        IEnumerable<Type> endpoints = typeof(T).Assembly.GetTypes()
            .Where(type =>
            {
                while (type.BaseType != null && type.BaseType != typeof(object))
                {
                    type = type.BaseType;
                }

                return type == typeof(EndpointBase);
            });

        foreach (Type endpoint in endpoints)
        {
            services.AddScoped(endpoint);
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
