using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Steganography.Common;

internal static class KestrelExtensions
{
    public static void ConfigureKestrel(this IWebHostBuilder builder)
    {
        builder.UseKestrelHttpsConfiguration();
        builder.ConfigureKestrel(static (context, serverOptions) =>
        {
            KestrelOptions options = new();
            context.Configuration.GetRequiredSection(KestrelOptions.Section).Bind(options);
            serverOptions.Limits.MaxRequestBodySize = options.Limits?.MaxRequestBodySize;
            context.ConfigureCertificate(serverOptions);
        });
    }

    private static void ConfigureCertificate(this WebHostBuilderContext context,
        KestrelServerOptions serverOptions)
    {
        IConfigurationSection kestrelSection = context.Configuration.GetRequiredSection(KestrelOptions.Section);
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
