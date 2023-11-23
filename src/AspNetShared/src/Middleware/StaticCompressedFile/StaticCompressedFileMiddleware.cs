using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace AspNetShared.Middleware.StaticCompressedFile;

public class StaticCompressedFileMiddleware : IMiddleware
{
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger _logger;
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
            IFileInfo fileInfo = _fileProvider.GetFileInfo($"{requestPath.Value}.{contentEncoding}");

            if (fileInfo.Exists)
            {
                return TryServeStaticCompressedFileAsync(context, fileInfo,
                    contentEncoding.ToString(), contentType, context.RequestAborted);
            }
        }

        return next(context);
    }

    private static bool HasEndpointDelegate(HttpContext context)
    {
        return context.GetEndpoint()?.RequestDelegate is not null;
    }

    private static bool IsValidMethod(HttpContext context)
    {
        return HttpMethods.IsGet(context.Request.Method);
    }

    private async Task TryServeStaticCompressedFileAsync(
        HttpContext httpContext,
        IFileInfo fileInfo,
        string contentEncoding,
        string contentType,
        CancellationToken cancellationToken)
    {
        HttpResponse response = httpContext.Response;
        ResponseHeaders responseHeaders = response.GetTypedHeaders();

        DateTimeOffset last = fileInfo.LastModified;
        // Truncate to seconds precision
        DateTimeOffset lastModified = new DateTimeOffset(last.Year, last.Month, last.Day,
            last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();

        MediaTypeHeaderValue contentTypeHeader = new(contentType)
        {
            Charset = "utf-8"
        };

        response.StatusCode = StatusCodes.Status200OK;

        responseHeaders.LastModified = lastModified;
        responseHeaders.ContentLength = fileInfo.Length;
        responseHeaders.ContentType = contentTypeHeader;
        responseHeaders.Headers.ContentEncoding = contentEncoding;

        StaticFileResponseContext responseContext = new(httpContext, fileInfo);
        _options.OnPrepareResponse(responseContext);

        await response.SendFileAsync(fileInfo, 0, fileInfo.Length, cancellationToken);
    }
}
