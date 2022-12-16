using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public abstract partial class Endpoint<TRequest> : EndpointBase
    where TRequest : notnull, new()
{
    internal sealed override async Task<IResult> ExecuteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        HttpContext = context;
        ValidationErrors = new List<string>();
        TRequest? request = default;

        try
        {
            if (HttpContext.Request.HasJsonContentType())
            {
                request = await HttpContext.Request.ReadFromJsonAsync<TRequest>(cancellationToken);
            }

            request ??= new TRequest();

            if (RequestTypeCache<TRequest>.BindAsync is not null)
            {
                try
                {
                    await RequestTypeCache<TRequest>.BindAsync(request, HttpContext,
                        ValidationErrors, cancellationToken);
                }
                catch (ModelBindingException e)
                {
                    ValidationErrors.Add(e.Message);
                }
            }

            if (ValidationErrors.Count > 0)
            {
                return ErrorResult("Model binding failed");
            }

            bool isValid = await ValidateAsync(request, cancellationToken);

            if (isValid)
            {
                return await HandleAsync(request, cancellationToken);
            }

            return ErrorResult("Validation failed");
        }
        catch (OperationCanceledException)
        {
            if (!HttpContext.Response.HasStarted)
            {
                return ErrorResult("Request was cancelled");
            }
        }

        return ErrorResult("An error occurred");
    }

    protected abstract Task<IResult> HandleAsync(TRequest request, CancellationToken cancellationToken);

    private async Task<bool> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        IValidator? validator = (IValidator?) HttpContext.RequestServices.GetService(typeof(IValidator<TRequest>));

        if (validator is null)
        {
            return true;
        }

        ValidationContext<TRequest> validationContext = new(request);
        ValidationResult validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

        if (validationResult.IsValid)
        {
            return true;
        }

        foreach (ValidationFailure error in validationResult.Errors)
        {
            ValidationErrors.Add(error.ErrorMessage);
        }

        return false;
    }
}
