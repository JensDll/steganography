using Microsoft.AspNetCore.Builder;

namespace MiniApi;

public static class EndpointAuthenticationDeclaration
{
    public static void Anonymous(params IEndpointConventionBuilder[] conventionBuilders)
    {
        foreach (IEndpointConventionBuilder conventionBuilder in conventionBuilders)
        {
            conventionBuilder.AllowAnonymous();
        }
    }
}
