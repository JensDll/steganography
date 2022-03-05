using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ApiBuilder;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapGet<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        return app.MapGet(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapPost<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        return app.MapPost(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapPut<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        return app.MapPut(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapDelete<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        return app.MapDelete(pattern, RequestHandler<TEndpoint>);
    }

    private static Task RequestHandler<TEndpoint>(HttpContext context)
        where TEndpoint : EndpointBase
    {
        TEndpoint endpoint = (TEndpoint) context.RequestServices.GetService(typeof(TEndpoint))!;
        return endpoint.ExecuteAsync(context);
    }
}
