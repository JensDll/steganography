using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;

    public StaticCompressedFileMiddleware(IWebHostEnvironment environment, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<StaticCompressedFileMiddleware>();
        _fileProvider = environment.WebRootFileProvider;
        _contentTypeProvider = StaticCompressedFileContentTypeProvider.Instance;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        PathString requestPath = context.Request.Path;

        using IDisposable? scope = _logger.ServingStaticCompressedFile(requestPath);

        RequestHeaders requestHeaders = context.Request.GetTypedHeaders();

        if (!Helper.TryGetContentEncoding(requestHeaders, out StringSegment contentEncoding))
        {
            _logger.AcceptEncodingMismatch();
        }
        else if (HasEndpointDelegate(context))
        {
            _logger.EndpointMatched();
        }
        else if (!IsValidMethod(context))
        {
            _logger.VerbMismatch(context.Request.Method);
        }
        else if (!_contentTypeProvider.TryGetContentType(requestPath, out string? contentType))
        {
            _logger.ContentTypeMismatch(contentType);
        }
        else
        {
            return TryServeStaticCompressedFile(next, context, requestPath, contentEncoding.ToString(), contentType);
        }

        return next(context);
    }

    private static bool HasEndpointDelegate(HttpContext context) => context.GetEndpoint()?.RequestDelegate is not null;

    private static bool IsValidMethod(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method);

    private async Task TryServeStaticCompressedFile(
        RequestDelegate next,
        HttpContext context,
        PathString subPath,
        string contentEncoding,
        string contentType)
    {
        IFileInfo fileInfo = _fileProvider.GetFileInfo($"{subPath}.{contentEncoding}");

        if (!fileInfo.Exists)
        {
            await next(context);
            return;
        }

        HttpResponse response = context.Response;
        ResponseHeaders responseHeaders = response.GetTypedHeaders();

        DateTimeOffset last = fileInfo.LastModified;
        // Truncate to seconds precision
        DateTimeOffset lastModified = new DateTimeOffset(last.Year, last.Month, last.Day,
            last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();

        MediaTypeHeaderValue contentTypeHeader = new(contentType)
        {
            Charset = "utf-8"
        };

        CacheControlHeaderValue cacheControlHeader = new()
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(365),
            Extensions = { new NameValueHeaderValue("immutable") }
        };

        response.StatusCode = StatusCodes.Status200OK;

        responseHeaders.LastModified = lastModified;
        responseHeaders.ContentLength = fileInfo.Length;
        responseHeaders.ContentType = contentTypeHeader;
        responseHeaders.CacheControl = cacheControlHeader;
        responseHeaders.Headers[HeaderNames.ContentEncoding] = contentEncoding;

        await response.SendFileAsync(fileInfo, 0, fileInfo.Length, context.RequestAborted);
    }
}
