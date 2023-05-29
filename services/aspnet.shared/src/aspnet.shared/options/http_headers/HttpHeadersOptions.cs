namespace aspnet.shared.options.http_headers;

public class HttpHeadersOptions
{
    public const string Section = "HttpHeaders";

    public string? ContentSecurityPolicy { get; set; }
}
