﻿using Serilog;
using ILogger = Serilog.ILogger;

namespace WebApi.Extensions;

public static class StartupExtensions
{
    public static ILogger AddSerilogLogger(this WebApplicationBuilder webBuilder)
    {
        webBuilder.Logging.ClearProviders();

        LoggerConfiguration loggerConfiguration = new();

        loggerConfiguration.WriteTo.Console();

        ILogger logger = loggerConfiguration.CreateLogger();

        webBuilder.Logging.AddSerilog(logger);
        webBuilder.Services.AddSingleton(logger);

        return logger;
    }

    public static string[] AllowedOrigins(this IConfiguration configuration)
    {
        List<string> allowedOrigins = new();
        configuration.GetSection("Cors:AllowedOrigins").Bind(allowedOrigins);
        return allowedOrigins.ToArray();
    }
}
