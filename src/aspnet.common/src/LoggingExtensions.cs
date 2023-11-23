using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;
using ILogger = Serilog.ILogger;

namespace aspnet.common.logging;

public static class LoggingExtensions
{
    private static ILogger? s_logger;

    public static ILogger AddSerilogLogger(this ILoggingBuilder builder)
    {
        if (s_logger is not null)
        {
            return s_logger;
        }

        builder.ClearProviders();

        LoggerConfiguration loggerConfiguration = new();

        loggerConfiguration.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .MinimumLevel.Information()
            .Filter.With<ExcludeStaticFileLogEventFilter>()
            .Filter.ByExcluding(Matching.WithProperty("Path", "/api/health"));

        s_logger = loggerConfiguration.CreateLogger();

        builder.AddSerilog(s_logger);
        builder.Services.AddSingleton(s_logger);

        return s_logger;
    }
}
