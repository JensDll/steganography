using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace aspnet.common.options.fluent_validation;

public static class FluentValidationValidateOptionsExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions, TValidator>(
        this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
        where TValidator : AbstractValidator<TOptions>
    {
        optionsBuilder.Services.TryAddSingleton<IValidator<TOptions>, TValidator>();

        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(serviceProvider =>
        {
            IValidator<TOptions> validator = serviceProvider.GetRequiredService<IValidator<TOptions>>();
            return new FluentValidationValidateOptions<TOptions>(validator);
        });

        return optionsBuilder;
    }
}
