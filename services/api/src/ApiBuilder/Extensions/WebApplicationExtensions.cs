using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ApiBuilder;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapGet<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        pattern = app.GetFullPattern(pattern);
        return app.MapGet(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapPost<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        pattern = app.GetFullPattern(pattern);
        return app.MapPost(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapPut<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        pattern = app.GetFullPattern(pattern);
        return app.MapPut(pattern, RequestHandler<TEndpoint>);
    }

    public static IEndpointConventionBuilder MapDelete<TEndpoint>(this WebApplication app, string pattern)
        where TEndpoint : EndpointBase
    {
        pattern = app.GetFullPattern(pattern);
        return app.MapDelete(pattern, RequestHandler<TEndpoint>);
    }

    private static Task<IResult> RequestHandler<TEndpoint>(HttpContext context, CancellationToken cancellationToken)
        where TEndpoint : EndpointBase
    {
        TEndpoint endpoint = (TEndpoint) context.RequestServices.GetService(typeof(TEndpoint))!;
        return endpoint.ExecuteAsync(context, cancellationToken);
    }

    private static ApiBuilderOptions GetOptions(this IHost app)
    {
        IOptions<ApiBuilderOptions> options =
            (IOptions<ApiBuilderOptions>) app.Services.GetService(typeof(IOptions<ApiBuilderOptions>))!;

        return options.Value;
    }

    private static string GetFullPattern(this IHost app, string pattern)
    {
        ApiBuilderOptions options = app.GetOptions();
        return string.IsNullOrEmpty(options.BaseUri) ? pattern : $"/{options.BaseUri}{pattern}";
    }
}
