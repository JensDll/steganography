using Amazon.CloudWatchLogs;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;
using ILogger = Serilog.ILogger;

const string corsDevPolicy = "cors:dev";
const string corsProdPolicy = "cors:prod";

WebApplicationBuilder webBuilder = WebApplication.CreateBuilder(args);

ILogger logger = CreateLogger(webBuilder);

webBuilder.Services.AddEndpointsApiExplorer();
webBuilder.Services.AddSwaggerGen();

webBuilder.Services.AddCors(options =>
{
    options.AddPolicy(corsDevPolicy, corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin();
    });
    options.AddPolicy(corsProdPolicy, corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin();
    });
});

WebApplication app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(app.Environment.IsDevelopment() ? corsDevPolicy : corsProdPolicy);

AmazonServiceDiscoveryClient client = new();

app.MapGet("/discover/{namespaceName}/{serviceName}",
    async ([FromRoute] string namespaceName, [FromRoute] string serviceName, CancellationToken cancellationToken) =>
    {
        try
        {
            DiscoverInstancesResponse? result = await client.DiscoverInstancesAsync(
                new DiscoverInstancesRequest
                {
                    NamespaceName = namespaceName,
                    ServiceName = serviceName
                }, cancellationToken);

            if (result is null || result.Instances.Count == 0)
            {
                logger.Information("Invalid response from service discovery");
                return Results.BadRequest();
            }

            int index = Random.Shared.Next(result.Instances.Count);
            result.Instances[index].Attributes.TryGetValue("AWS_INSTANCE_IPV4", out string? ipv4);

            if (ipv4 is not null)
            {
                return Results.Text(ipv4);
            }

            logger.Information("Selected instance did not have an IPV4 address");
            return Results.BadRequest();
        }
        catch (Exception e)
        {
            logger.Information("Failed to discover instances: {Message}", e.Message);
            return Results.BadRequest();
        }
    }
);

app.Run();

static ILogger CreateLogger(WebApplicationBuilder webBuilder)
{
    webBuilder.Logging.ClearProviders();

    JsonFormatter textFormatter = new(Environment.NewLine);

    LoggerConfiguration loggerConfiguration = new();

    if (webBuilder.Environment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.WriteTo.File(textFormatter,
            path: Path.Combine(webBuilder.Environment.ContentRootPath, "Logs", "log.txt"),
            rollingInterval: RollingInterval.Day);
    }
    else
    {
        CloudWatchSinkOptions options = new()
        {
            LogGroupName = $"app/serviceDiscovery",
            CreateLogGroup = true,
            TextFormatter = textFormatter,
            MinimumLogEventLevel = LogEventLevel.Information,
            LogStreamNameProvider =
                new ConfigurableLogStreamNameProvider(
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH.mm.ss.fff"),
                    false, false),
            LogGroupRetentionPolicy = LogGroupRetentionPolicy.OneWeek
        };
        AmazonCloudWatchLogsClient client = new();
        loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
    }

    ILogger logger = loggerConfiguration.CreateLogger();

    webBuilder.Logging.AddSerilog(logger);
    webBuilder.Services.AddSingleton(logger);

    return logger;
}
