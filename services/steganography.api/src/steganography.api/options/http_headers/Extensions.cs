using aspnet.shared.options.fluent_validation;

namespace steganography.api.options.http_headers;

public static class Extensions
{
    public static void AddHttpHeadersOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<HttpHeadersOptions>()
            .BindConfiguration(HttpHeadersOptions.Section)
            .ValidateFluentValidation<HttpHeadersOptions, HttpHeadersOptionsValidator>()
            .ValidateOnStart();
    }
}
