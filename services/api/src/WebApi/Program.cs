using ApiBuilder;
using Domain;
using Microsoft.AspNetCore.Http.Features;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;
using static ApiBuilder.EndpointAuthenticationDeclaration;

const string corsDevPolicy = "cors:dev";
const string corsProdPolicy = "cors:prod";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800; // 50 MB;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
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
builder.Services.AddDataProtection();
builder.Services.AddDomain();
builder.Services.AddEndpoints<Program>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50 MB
});
builder.Services.Configure<ApiBuilderOptions>(options =>
{
    options.BaseUri = "api";
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? corsDevPolicy : corsProdPolicy);

Anonymous(
    app.MapPost<EncodeText>("/codec/encode/text"),
    app.MapPost<EncodeBinary>("/codec/encode/binary"),
    app.MapPost<Decode>("/codec/decode")
);

app.Run();
