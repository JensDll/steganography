using FluentValidation;

namespace AspNetShared;

internal sealed class HttpHeadersOptionsValidator : AbstractValidator<HttpHeadersOptions>
{
    public HttpHeadersOptionsValidator()
    {
        RuleFor(static options => options.ContentSecurityPolicy).NotEmpty();
    }
}
