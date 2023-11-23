using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetShared;

public static class HttpHeadersOptionsExtensions
{
    public static void AddHttpHeadersOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<HttpHeadersOptions>()
            .BindConfiguration(HttpHeadersOptions.Section)
            .ValidateFluentValidation<HttpHeadersOptions, HttpHeadersOptionsValidator>()
            .ValidateOnStart();
    }
}
