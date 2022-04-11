using System.Security.Cryptography.X509Certificates;
using ApiBuilder;
using Domain;
using WebApi.Common;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;

const string corsDevPolicy = "cors:dev";
const string corsProdPolicy = "cors:prod";

WebApplicationBuilder webBuilder = WebApplication.CreateBuilder(args);

webBuilder.AddSerilogLogger();

webBuilder.Services.AddEndpointsApiExplorer();
webBuilder.Services.AddSwaggerGen();
webBuilder.Services.AddCors(options =>
{
    options.AddPolicy(corsDevPolicy, corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin();
    });
});
webBuilder.Services.AddDomain();
webBuilder.Services.AddEndpoints<Program>();
webBuilder.Services.Configure<ApiBuilderOptions>(options =>
{
    options.BaseUri = "api";
});

webBuilder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 60 * 1024 * 1024; // 60 MB

    if (EnvironmentHelper.IsDocker())
    {
        options.ConfigureHttpsDefaults(httpsOptions =>
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

EndpointAuthenticationDeclaration.Anonymous(
    app.MapPost<EncodeText>("/codec/encode/text"),
    app.MapPost<EncodeBinary>("/codec/encode/binary"),
    app.MapPost<Decode>("/codec/decode")
);

app.Run();
