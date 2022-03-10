using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public interface IBindRequest
{
    ValueTask BindAsync(HttpContext context, List<string> validationErrors);
}
