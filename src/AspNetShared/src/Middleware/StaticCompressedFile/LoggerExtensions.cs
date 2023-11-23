using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace AspNetShared.Middleware.StaticCompressedFile;

internal static class LoggerExtensions
{
    private static readonly Func<ILogger, PathString, IDisposable?> s_servingStaticCompressedFile =
        LoggerMessage.DefineScope<PathString>("Serving static compressed file: {Path}");

    private static readonly Action<ILogger, Exception?> s_acceptEncodingMismatch =
        LoggerMessage.Define(LogLevel.Debug, new EventId(1, nameof(AcceptEncodingMismatch)),
            $"Skipped as the request does not have an {HeaderNames.AcceptEncoding} header");

    private static readonly Action<ILogger, Exception?> s_pathMismatch =
        LoggerMessage.Define(LogLevel.Debug, new EventId(2, nameof(PathMismatch)),
            "Skipped as the request path does start with the configured path");

    private static readonly Action<ILogger, Exception?> s_endpointMatched =
        LoggerMessage.Define(LogLevel.Debug, new EventId(3, nameof(EndpointMatched)),
            "Skipped as the request already matched an endpoint");

    private static readonly Action<ILogger, string, Exception?> s_verbMismatch =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4, nameof(VerbMismatch)),
            "Skipped as {RequestMethod} requests are not supported");

    private static readonly Action<ILogger, string?, Exception?> s_contentTypeMismatch =
        LoggerMessage.Define<string?>(LogLevel.Debug, new EventId(5, nameof(ContentTypeMismatch)),
            "Skipped as the {ContentType} content type is not supported");

    internal static IDisposable? ServingStaticCompressedFile(this ILogger logger, PathString path)
    {
        return s_servingStaticCompressedFile(logger, path);
    }

    internal static void AcceptEncodingMismatch(this ILogger logger)
    {
        s_acceptEncodingMismatch(logger, null);
    }

    internal static void PathMismatch(this ILogger logger)
    {
        s_pathMismatch(logger, null);
    }

    internal static void EndpointMatched(this ILogger logger)
    {
        s_endpointMatched(logger, null);
    }

    internal static void VerbMismatch(this ILogger logger, string requestMethod)
    {
        s_verbMismatch(logger, requestMethod, null);
    }

    internal static void ContentTypeMismatch(this ILogger logger, string? contentType)
    {
        s_contentTypeMismatch(logger, contentType, null);
    }
}
