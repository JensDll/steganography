using Microsoft.AspNetCore.Http;

namespace MiniApi;

public abstract class EndpointBase
{
    internal HttpContext RequestHttpContext = null!;

    protected HttpContext HttpContext => RequestHttpContext;
}
