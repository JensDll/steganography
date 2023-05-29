namespace steganography.api.features.common;

public static class Extensions
{
    public static IEndpointRouteBuilder MapCommonFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder auxiliary = endpoints.MapGroup("/").WithTags("Common");
        auxiliary.MapGet("/health", () => TypedResults.Ok());

        return endpoints;
    }
}
