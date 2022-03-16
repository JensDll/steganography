using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public abstract partial class Endpoint<TRequest, TResponse>
    where TRequest : notnull, new()
    where TResponse : notnull, new()
{
    protected Task SendTextAsync(string text)
    {
        HttpContext.Response.ContentType = "text/plain";
        return HttpContext.Response.WriteAsync(text);
    }

    protected ValueTask SendTextAsync(ReadOnlyMemory<byte> text)
    {
        HttpContext.Response.ContentType = "text/plain";
        return HttpContext.Response.Body.WriteAsync(text);
    }

    protected Task SendAsync(TResponse response)
    {
        return HttpContext.Response.WriteAsJsonAsync(response);
    }

    protected Task SendValidationErrorAsync(string message)
    {
        if (HttpContext.Response.StatusCode is < 400 or >= 500)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        return HttpContext.Response.WriteAsJsonAsync(new
        {
            HttpContext.Response.StatusCode,
            Message = message,
            Errors = ValidationErrors
        });
    }
}
