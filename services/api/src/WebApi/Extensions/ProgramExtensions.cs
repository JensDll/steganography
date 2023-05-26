using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ILogger = Serilog.ILogger;

namespace WebApi.Extensions;

public static class ProgramExtensions
{
    public static ILogger AddSerilogLogger(this ILoggingBuilder builder)
    {
        builder.ClearProviders();

        LoggerConfiguration loggerConfiguration = new();
        loggerConfiguration.MinimumLevel.Information();
        loggerConfiguration.WriteTo.Console();

        ILogger logger = loggerConfiguration.CreateLogger();

        builder.AddSerilog(logger);
        builder.Services.AddSingleton(logger);

        return logger;
    }

    public static void AddDefaultJson(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile(Path.Join("Properties", "appsettings.json"),
            optional: false, reloadOnChange: false);
    }

    public static string[] AllowedOrigins(this IConfiguration configuration)
    {
        List<string> allowedOrigins = new();
        configuration.GetSection("Cors:AllowedOrigins").Bind(allowedOrigins);
        return allowedOrigins.ToArray();
    }

    public static void ConfigureCertificate(this WebHostBuilderContext context, KestrelServerOptions serverOptions)
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            return;
        }

        IConfigurationSection kestrelSection = context.Configuration.GetSection("Kestrel");

        serverOptions.Configure(kestrelSection)
            .Endpoint("Https", endpoint =>
            {
                string certPath =
                    endpoint.ConfigSection.GetValue<string>("Path") ??
                    throw new InvalidOperationException(
                        "Failed to find certificate path on https endpoint configuration");

                string keyPath =
                    endpoint.ConfigSection.GetValue<string>("KeyPath") ??
                    throw new InvalidOperationException(
                        "Failed to find key path on https endpoint configuration");

                X509Certificate2 certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);

                endpoint.ListenOptions.UseHttps(certificate, httpsOptions =>
                {
                    X509Certificate2Collection chain = new();
                    chain.ImportFromPemFile(certPath);
                    httpsOptions.ServerCertificateChain = chain;
                });
            });
    }
}
