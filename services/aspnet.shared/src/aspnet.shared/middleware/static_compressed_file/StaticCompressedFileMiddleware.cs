using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace aspnet.shared.middleware.static_compressed_file;

public class StaticCompressedFileMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly StaticCompressedFileOptions _options;

    public StaticCompressedFileMiddleware(IOptions<StaticCompressedFileOptions> options,
        IWebHostEnvironment environment, ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _logger = loggerFactory.CreateLogger<StaticCompressedFileMiddleware>();
        _fileProvider = environment.WebRootFileProvider;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
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
        else if (!Helper.TryMatchPath(requestPath, _options.RequestPath, out PathString subPath))
        {
            _logger.PathMismatch();
        }
        else if (!_contentTypeProvider.TryGetContentType(subPath.Value!, out string? contentType))
        {
            _logger.ContentTypeMismatch(contentType);
        }
        else
        {
            IFileInfo fileInfo = _fileProvider.GetFileInfo($"{subPath.Value}.{contentEncoding}");

            if (fileInfo.Exists)
            {
                return TryServeStaticCompressedFileAsync(context.Response, fileInfo,
                    contentEncoding.ToString(), contentType, context.RequestAborted);
            }
        }

        return next(context);
    }

    private static bool HasEndpointDelegate(HttpContext context) => context.GetEndpoint()?.RequestDelegate is not null;

    private static bool IsValidMethod(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method);

    private static async Task TryServeStaticCompressedFileAsync(
        HttpResponse response,
        IFileInfo fileInfo,
        string contentEncoding,
        string contentType,
        CancellationToken cancellationToken)
    {
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

        await response.SendFileAsync(fileInfo, 0, fileInfo.Length, cancellationToken);
    }
}
