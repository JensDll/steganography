using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public abstract partial class Endpoint<TRequest>
    where TRequest : notnull, new()
{
    protected IResult ErrorResult(string message)
    {
        return Results.BadRequest(new
        {
            StatusCode = 400,
            Message = message,
            Errors = ValidationErrors
        });
    }
}
