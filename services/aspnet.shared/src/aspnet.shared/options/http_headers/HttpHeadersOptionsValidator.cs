using FluentValidation;

namespace aspnet.shared.options.http_headers;

public class HttpHeadersOptionsValidator : AbstractValidator<HttpHeadersOptions>
{
    public HttpHeadersOptionsValidator()
    {
        RuleFor(static options => options.ContentSecurityPolicy).NotEmpty();
    }
}
