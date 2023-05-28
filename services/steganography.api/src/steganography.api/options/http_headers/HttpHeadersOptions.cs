namespace steganography.api.options.http_headers;

public class HttpHeadersOptions
{
    public const string Section = "HttpHeaders";

    public string? ContentSecurityPolicyPolicy { get; set; }

    public string? ContentSecurityPolicyPolicyReportOnly { get; set; }
}
