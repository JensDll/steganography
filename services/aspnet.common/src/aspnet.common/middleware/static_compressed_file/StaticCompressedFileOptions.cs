using aspnet.common.options.http_headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace aspnet.common.middleware.static_compressed_file;

public class StaticCompressedFileOptions : StaticFileOptions
{
    public static readonly Action<StaticFileResponseContext> DefaultOnPrepareResponse = static context =>
    {
        IHeaderDictionary headers = context.Context.Response.Headers;
        headers.CacheControl = "public,max-age=31536000,immutable";
        headers.XContentTypeOptions = "nosniff";
    };

    public static readonly Action<StaticFileResponseContext> IndexOnPrepareResponse = static context =>
    {
        HttpHeadersOptions options =
            context.Context.RequestServices.GetRequiredService<IOptions<HttpHeadersOptions>>().Value;

        IHeaderDictionary headers = context.Context.Response.Headers;
        headers.CacheControl = "private,no-store,no-cache,max-age=0,must-revalidate";
        headers.XContentTypeOptions = "nosniff";
        headers.XXSSProtection = "0";
        headers.ContentSecurityPolicy = options.ContentSecurityPolicy;
    };
}
