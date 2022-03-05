using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
    where TRequest : notnull, new()
    where TResponse : notnull, new()
{
    protected Task SendTextAsync(string text)
    {
        HttpContext.Response.ContentType = "text/plain";
        return HttpContext.Response.WriteAsync(text);
    }

    protected ValueTask SendTextAsync(byte[] text)
    {
        HttpContext.Response.ContentType = "text/plain";
        return HttpContext.Response.Body.WriteAsync(text);
    }

    protected Task SendAsync(TResponse response)
    {
        HttpContext.Response.ContentType = "application/json";
        return HttpContext.Response.WriteAsJsonAsync(response);
    }

    protected Task SendValidationErrorAsync(string message)
    {
        HttpContext.Response.StatusCode = 400;
        return HttpContext.Response.WriteAsJsonAsync(new
        {
            HttpContext.Response.StatusCode,
            Message = message,
            Errors = ValidationErrors
        });
    }

    internal override async Task ExecuteAsync(HttpContext context)
    {
        HttpContext = context;
        ValidationErrors = new List<string>();

        TRequest? request = default;

        if (HttpContext.Request.HasJsonContentType())
        {
            request = await HttpContext.Request.ReadFromJsonAsync<TRequest>();
        }

        request ??= new TRequest();

        if (RequestTypeCache<TRequest>.BindAsync != null)
        {
            try
            {
                await (Task) RequestTypeCache<TRequest>.BindAsync.Invoke(request,
                    new object[] {HttpContext, ValidationErrors})!;
            }
            catch (ModelBindingException e)
            {
                ValidationErrors.Add(e.Message);
            }
        }

        if (ValidationErrors.Any())
        {
            await SendValidationErrorAsync("Model binding failed");
            return;
        }

        bool isValid = await ValidateAsync(request);

        if (isValid)
        {
            await HandleAsync(request);
        }
    }

    protected abstract Task HandleAsync(TRequest request);

    private async Task<bool> ValidateAsync(TRequest request)
    {
        IValidator? validator = (IValidator?) HttpContext.RequestServices.GetService(typeof(IValidator<TRequest>));

        if (validator is null)
        {
            return true;
        }

        ValidationContext<object> validationContext = new(request);
        ValidationResult validationResult = await validator.ValidateAsync(validationContext);

        if (validationResult.IsValid)
        {
            return true;
        }

        foreach (ValidationFailure error in validationResult.Errors)
        {
            ValidationErrors.Add(error.ErrorMessage);
        }

        await SendValidationErrorAsync("Validation failed");

        return false;
    }
}

public abstract class EndpointWithoutRequest<TResponse> : Endpoint<EmptyRequest, TResponse>
    where TResponse : notnull, new()
{
    protected sealed override Task HandleAsync(EmptyRequest _)
    {
        return HandleAsync();
    }

    protected abstract Task HandleAsync();
}

public abstract class EndpointWithoutResponse<TRequest> : Endpoint<TRequest, object>
    where TRequest : notnull, new()
{
}

public abstract class EmptyEndpoint : EndpointWithoutRequest<object>
{
}
