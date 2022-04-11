using Amazon.CloudWatchLogs;
using ApiBuilder;
using Domain;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using Serilog.Sinks.AwsCloudWatch.LogStreamNameProvider;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;
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
webBuilder.Services.AddDomain();
webBuilder.Services.AddEndpoints<Program>();

webBuilder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 60 * 1024 * 1024; // 60 MB
});

WebApplication app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(app.Environment.IsDevelopment() ? corsDevPolicy : corsProdPolicy);

EndpointAuthenticationDeclaration.Anonymous(
    app.MapPost<EncodeText>("/codec/encode/text"),
    app.MapPost<EncodeBinary>("/codec/encode/binary"),
    app.MapPost<Decode>("/codec/decode"),
    app.MapGet("/aws", () => "Hello AWS")
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
        AmazonCloudWatchLogsClient client = new();
        loggerConfiguration.WriteTo.AmazonCloudWatch(options, client);
    }

    ILogger logger = loggerConfiguration.CreateLogger();

    webBuilder.Logging.AddSerilog(logger);
    webBuilder.Services.AddSingleton(logger);

    return logger;
}
