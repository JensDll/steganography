using System.Security.Cryptography.X509Certificates;
using ApiBuilder;
using Domain;
using WebApi.Common;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;
using static ApiBuilder.EndpointAuthenticationDeclaration;

const string corsDevPolicy = "cors:dev";

WebApplicationBuilder webBuilder = WebApplication.CreateBuilder(args);

webBuilder.AddSerilogLogger();

if (webBuilder.Environment.IsDevelopment())
{
    webBuilder.Services.AddEndpointsApiExplorer();
    webBuilder.Services.AddSwaggerGen();
}

webBuilder.Services.AddCors(options =>
{
    options.AddPolicy(corsDevPolicy, corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin();
    });
});
webBuilder.Services.AddDomain();
webBuilder.Services.AddEndpoints<Program>();

webBuilder.WebHost.ConfigureKestrel(kestrelOptions =>
{
    kestrelOptions.Limits.MaxRequestBodySize = 60 * 1024 * 1024; // 60 MB

    if (!webBuilder.Environment.IsDevelopment())
    {
        kestrelOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            string certPath = Path.Combine(webBuilder.Environment.ContentRootPath, "cert.pem");
            string keyPath = Path.Combine(webBuilder.Environment.ContentRootPath, "key.pem");

            httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        });
    }
});

WebApplication app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(corsDevPolicy);
}

Anonymous(
    app.MapPost<EncodeText>("/api/codec/encode/text"),
    app.MapPost<EncodeBinary>("/api/codec/encode/binary"),
    app.MapPost<Decode>("/api/codec/decode"),
    app.MapGet("/api/health", () => Results.Ok())
);

app.Run();
