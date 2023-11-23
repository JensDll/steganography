using aspnet.common.options.fluent_validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace aspnet.common.options.http_headers;

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
