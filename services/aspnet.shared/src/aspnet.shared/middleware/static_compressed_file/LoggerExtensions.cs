using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace aspnet.shared.middleware.static_compressed_file;

internal static class LoggerExtensions
{
    private static readonly Func<ILogger, PathString, IDisposable?> s_servingStaticCompressedFile;

    private static readonly Action<ILogger, Exception?> s_acceptEncodingMismatch;

    private static readonly Action<ILogger, Exception?> s_pathMismatch;

    private static readonly Action<ILogger, Exception?> s_endpointMatched;

    private static readonly Action<ILogger, string, Exception?> s_verbMismatch;

    private static readonly Action<ILogger, string?, Exception?> s_contentTypeMismatch;

    static LoggerExtensions()
    {
        s_servingStaticCompressedFile = LoggerMessage.DefineScope<PathString>(
            "Serving static compressed file: {Path}");

        s_acceptEncodingMismatch = LoggerMessage.Define(LogLevel.Information,
            new EventId(1, nameof(AcceptEncodingMismatch)),
            $"Skipped as the request does not have an {HeaderNames.AcceptEncoding} header");

        s_pathMismatch = LoggerMessage.Define(LogLevel.Information,
            new EventId(2, nameof(PathMismatch)),
            "Skipped as the request path does start with the configured path");

        s_endpointMatched = LoggerMessage.Define(LogLevel.Information,
            new EventId(3, nameof(EndpointMatched)),
            "Skipped as the request already matched an endpoint");

        s_verbMismatch = LoggerMessage.Define<string>(LogLevel.Information,
            new EventId(4, nameof(VerbMismatch)),
            "Skipped as {RequestMethod} requests are not supported");

        s_contentTypeMismatch = LoggerMessage.Define<string?>(LogLevel.Information,
            new EventId(5, nameof(ContentTypeMismatch)),
            "Skipped as the {ContentType} content type is not supported");
    }

    internal static IDisposable? ServingStaticCompressedFile(this ILogger logger, PathString path)
        => s_servingStaticCompressedFile(logger, path);

    internal static void AcceptEncodingMismatch(this ILogger logger)
        => s_acceptEncodingMismatch(logger, null);

    internal static void PathMismatch(this ILogger logger)
        => s_pathMismatch(logger, null);

    internal static void EndpointMatched(this ILogger logger)
        => s_endpointMatched(logger, null);

    internal static void VerbMismatch(this ILogger logger, string requestMethod)
        => s_verbMismatch(logger, requestMethod, null);

    internal static void ContentTypeMismatch(this ILogger logger, string? contentType)
        => s_contentTypeMismatch(logger, contentType, null);
}
