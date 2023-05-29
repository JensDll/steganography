using FluentValidation;

namespace aspnet.common.options.http_headers;

public class HttpHeadersOptionsValidator : AbstractValidator<HttpHeadersOptions>
{
    public HttpHeadersOptionsValidator()
    {
        RuleFor(static options => options.ContentSecurityPolicy).NotEmpty();
    }
}
