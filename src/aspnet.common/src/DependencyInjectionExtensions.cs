using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace aspnet.common;

public static class DependencyInjectionExtensions
{
    public static void AddDefaultJson(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile(Path.Join("Properties", "appsettings.json"),
            false, true);
    }

    public static IServiceCollection AddAspNetShared(this IServiceCollection services)
    {
        return services;
    }

    public static void ConfigureCertificate(this WebHostBuilderContext context, KestrelServerOptions serverOptions)
    {
        IConfigurationSection kestrelSection =
            context.Configuration.GetRequiredSection(options.kestrel.KestrelServerOptions.Section);

        IConfigurationSection httpsSection = kestrelSection.GetSection("Endpoints:Https");

        if (!httpsSection.Exists())
        {
            return;
        }

        KestrelConfigurationLoader loader = serverOptions.Configure(kestrelSection);

        loader.Endpoint("Https", endpoint =>
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
