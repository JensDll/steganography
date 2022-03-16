using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public abstract class EndpointBase
{
    protected internal HttpContext HttpContext { get; internal set; } = null!;

    protected internal List<string> ValidationErrors { get; internal set; } = null!;

    internal abstract Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken);
}
