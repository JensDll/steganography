using aspnet.shared.options.http_headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileOptions
{
    private static readonly CacheControlHeaderValue s_cacheHeader = new()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(365),
        Extensions = { new NameValueHeaderValue("immutable") }
    };

    private static readonly CacheControlHeaderValue s_noCacheHeader = new()
    {
        Private = true,
        NoStore = true,
        NoCache = true,
        MustRevalidate = true,
        MaxAge = TimeSpan.Zero
    };

    public PathString RequestPath { get; set; } = PathString.Empty;

    public static ResponseHeaders AddCacheBustingHeaders(HttpResponse response)
    {
        ResponseHeaders responseHeaders = response.GetTypedHeaders();

        responseHeaders.CacheControl = s_cacheHeader;
        responseHeaders.Headers[HeaderNames.XContentTypeOptions] = "nosniff";

        return responseHeaders;
    }

    public static void AddIndexHtmlHeaders(HttpContext context)
    {
        ResponseHeaders responseHeaders = context.Response.GetTypedHeaders();

        HttpHeadersOptions options =
            context.RequestServices.GetRequiredService<IOptions<HttpHeadersOptions>>().Value;

        responseHeaders.CacheControl = s_noCacheHeader;
        responseHeaders.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
        responseHeaders.Headers.XXSSProtection = "0";
        responseHeaders.Headers.ContentSecurityPolicy = options.ContentSecurityPolicy;
    }
}
