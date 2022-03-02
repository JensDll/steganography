using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MiniApi;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapGet<TRequest>(this WebApplication app, string pattern)
        where TRequest : Request, new()
    {
        return app.MapGet(pattern, RequestHandler<TRequest>);
    }

    public static IEndpointConventionBuilder MapGet<TRequest, TResponse>(this WebApplication app, string pattern)
        where TRequest : Request, new()
        where TResponse : notnull
    {
        return app.MapGet(pattern, RequestHandler<TRequest, TResponse>);
    }

    public static IEndpointConventionBuilder MapPost<TRequest>(this WebApplication app, string pattern)
        where TRequest : Request, new()
    {
        return app.MapPost(pattern, RequestHandler<TRequest>);
    }

    public static IEndpointConventionBuilder MapPost<TRequest, TResponse>(this WebApplication app, string pattern)
        where TRequest : Request, new()
        where TResponse : notnull
    {
        return app.MapPost(pattern, RequestHandler<TRequest, TResponse>);
    }

    private static async Task RequestHandler<TRequest>(HttpContext context)
        where TRequest : Request, new()
    {
        Endpoint<TRequest> endpoint =
            (Endpoint<TRequest>) context.RequestServices.GetService(typeof(Endpoint<TRequest>))!;

        (bool IsSuccess, TRequest Request)
            result = await ValidateAsync<Endpoint<TRequest>, TRequest>(context, endpoint);

        if (result.IsSuccess)
        {
            await endpoint.HandleAsync(result.Request);
        }
    }

    private static async Task RequestHandler<TRequest, TResponse>(HttpContext context)
        where TRequest : Request, new()
        where TResponse : notnull
    {
        Endpoint<TRequest, TResponse> endpoint =
            (Endpoint<TRequest, TResponse>) context.RequestServices.GetService(typeof(Endpoint<TRequest, TResponse>))!;

        (bool IsSuccess, TRequest Request) result =
            await ValidateAsync<Endpoint<TRequest, TResponse>, TRequest>(context, endpoint);

        if (result.IsSuccess)
        {
            await endpoint.HandleAsync(result.Request);
        }
    }

    private static async Task<(bool IsSuccess, TRequest Request)> ValidateAsync<TEndpoint, TRequest>(
        HttpContext context,
        TEndpoint endpoint)
        where TEndpoint : EndpointBase
        where TRequest : Request, new()
    {
        endpoint.RequestHttpContext = context;

        List<string> validationErrors = new();
        context.Items[nameof(validationErrors)] = validationErrors;

        TRequest? request = context.Request.HasJsonContentType()
            ? (TRequest?) await context.Request.ReadFromJsonAsync(typeof(TRequest))
            : null;
        request ??= new TRequest();

        await request.BindAsync(context, validationErrors);

        if (validationErrors.Count > 0)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Model binding failed",
                errors = validationErrors
            });

            return (false, request);
        }

        IValidator? validator = (IValidator?) context.RequestServices.GetService(typeof(IValidator<TRequest>));

        if (validator == null)
        {
            return (true, request);
        }

        ValidationContext<object> validationContext = new(request);
        ValidationResult? validationResult = await validator.ValidateAsync(validationContext);

        if (validationResult.IsValid)
        {
            return (true, request);
        }

        foreach (ValidationFailure? error in validationResult.Errors)
        {
            validationErrors.Add(error.ErrorMessage);
        }

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Validation failed",
            errors = validationErrors
        });

        return (false, request);
    }
}
