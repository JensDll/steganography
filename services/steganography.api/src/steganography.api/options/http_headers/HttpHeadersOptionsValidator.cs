using FluentValidation;

namespace steganography.api.options.http_headers;

public class HttpHeadersOptionsValidator : AbstractValidator<HttpHeadersOptions>
{
    public HttpHeadersOptionsValidator()
    {
        RuleFor(static options => options.ContentSecurityPolicyPolicy).NotEmpty()
            .When(options => options.ContentSecurityPolicyPolicyReportOnly is null);
    }
}
