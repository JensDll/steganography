using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace aspnet.shared;

public static class DependencyInjection
{
    public static void AddDefaultJson(this IConfigurationBuilder builder)
    {
        builder.AddJsonFile(Path.Join("Properties", "appsettings.json"),
            optional: false, reloadOnChange: true);
    }

    public static IServiceCollection AddAspNetShared(this IServiceCollection services)
    {
        return services;
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
                string certPath = endpoint.ConfigSection.GetValue<string>("Path")
                                  ?? throw new InvalidOperationException(
                                      "Failed to find certificate path on https endpoint configuration");

                string keyPath = endpoint.ConfigSection.GetValue<string>("KeyPath") ??
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
