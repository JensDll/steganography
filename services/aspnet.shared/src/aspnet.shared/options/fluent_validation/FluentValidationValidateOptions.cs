using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace aspnet.shared.options.fluent_validation;

public class FluentValidationValidateOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IValidator<TOptions> _validator;

    public FluentValidationValidateOptions(IValidator<TOptions> validator)
    {
        _validator = validator;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ValidationResult result = _validator.Validate(options);

        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        IEnumerable<string> errors = result.Errors.Select(failure =>
            $"{options.GetType().Name} validation failed for '{failure.PropertyName}': {failure.ErrorMessage}");

        return ValidateOptionsResult.Fail(errors);
    }
}
