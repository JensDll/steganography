using ApiBuilder;
using Domain;
using WebApi.Extensions;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;
using static ApiBuilder.EndpointAuthenticationDeclaration;

const string corsDevPolicy = "cors:dev";
const string corsProdPolicy = "cors:prod";

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

    options.AddPolicy(corsProdPolicy, corsBuilder =>
    {
        corsBuilder.WithOrigins("https://imagehiding.com").AllowAnyMethod();
    });
});

webBuilder.Services.AddDomain();

webBuilder.Services.AddEndpoints<Program>();

webBuilder.WebHost.ConfigureKestrel(kestrelOptions =>
{
    kestrelOptions.Limits.MaxRequestBodySize = 60 * 1024 * 1024; // 60 MB
});

WebApplication app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(corsDevPolicy);
}

app.UseCors(corsProdPolicy);

Anonymous(
    app.MapPost<EncodeText>("/codec/encode/text"),
    app.MapPost<EncodeBinary>("/codec/encode/binary"),
    app.MapPost<Decode>("/codec/decode"),
    app.MapGet("/health", () => Results.Ok())
);

app.Run();
