using Amazon;
using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;
using ILogger = Serilog.ILogger;

namespace WebApi.Extensions;

public static class StartupExtensions
{
    private const string _deployment = "Deployment";

    public static ILogger AddSerilogLogger(this WebApplicationBuilder webBuilder)
    {
        webBuilder.Logging.ClearProviders();

        JsonFormatter textFormatter = new(Environment.NewLine);
        LoggerConfiguration loggerConfiguration = new();

        if (webBuilder.Environment.IsDeployment())
        {
            // Exclude logs from the health check endpoint
            loggerConfiguration.Filter.ByExcluding(Matching.WithProperty<string>("RequestPath",
                path => path == "/api/health"));

            CloudWatchSinkOptions options = new()
            {
                LogGroupName = "app/api",
                CreateLogGroup = true,
                TextFormatter = textFormatter,
                MinimumLogEventLevel = LogEventLevel.Information,
                LogStreamNameProvider =
                    new ConfigurableLogStreamNameProvider(
                        DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss.fff"),
                        false, false),
                LogGroupRetentionPolicy = LogGroupRetentionPolicy.OneWeek
            };
            AmazonCloudWatchLogsClient client = new(RegionEndpoint.EUCentral1);

            loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
        }
        else
        {
            loggerConfiguration.WriteTo.Console();
            loggerConfiguration.WriteTo.File(textFormatter,
                path: Path.Combine(webBuilder.Environment.ContentRootPath, "Logs", "log.txt"),
                rollingInterval: RollingInterval.Day);
        }

        ILogger logger = loggerConfiguration.CreateLogger();

        webBuilder.Logging.AddSerilog(logger);
        webBuilder.Services.AddSingleton(logger);

        return logger;
    }

    public static bool IsDeployment(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment(_deployment);
    }
}
