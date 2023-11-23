using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;

namespace AspNetShared;

public static class KestrelExtensions
{
    public static void ConfigureKestrel(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrelHttpsConfiguration();
        builder.WebHost.ConfigureKestrel(static (context, serverOptions) =>
        {
            KestrelServerOptions options = new();
            context.Configuration.GetRequiredSection(KestrelServerOptions.Section).Bind(options);
            serverOptions.Limits.MaxRequestBodySize = options.Limits?.MaxRequestBodySize;
            context.ConfigureCertificate(serverOptions);
        });
    }

    private static void ConfigureCertificate(this WebHostBuilderContext context,
        Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions serverOptions)
    {
        IConfigurationSection kestrelSection =
            context.Configuration.GetRequiredSection(KestrelServerOptions.Section);

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
                X509Certificate2Collection chain = [];
                chain.ImportFromPemFile(certPath);
                httpsOptions.ServerCertificateChain = chain;
            });
        });
    }
}
