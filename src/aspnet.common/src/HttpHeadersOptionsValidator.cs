using FluentValidation;

namespace aspnet.common.options.http_headers;

internal sealed class HttpHeadersOptionsValidator : AbstractValidator<HttpHeadersOptions>
{
    public HttpHeadersOptionsValidator()
    {
        RuleFor(static options => options.ContentSecurityPolicy).NotEmpty();
    }
}
