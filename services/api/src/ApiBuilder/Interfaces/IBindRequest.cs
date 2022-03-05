using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public interface IBindRequest
{
    Task BindAsync(HttpContext context, List<string> validationErrors);
}
