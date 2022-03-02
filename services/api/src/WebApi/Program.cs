using Domain;
using Microsoft.AspNetCore.Http.Features;
using MiniApi;
using WebApi.Endpoints.Codec.Request;
using static MiniApi.EndpointAuthenticationDeclaration;

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
    options.AddPolicy(corsDevPolicy, corsBuilder => { corsBuilder.AllowAnyOrigin(); });
    options.AddPolicy(corsProdPolicy, corsBuilder => { corsBuilder.AllowAnyOrigin(); });
});
builder.Services.AddDataProtection();
builder.Services.AddDomain();
builder.Services.AddEndpoints<Program>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50 MB
});

WebApplication app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/codec"))
    {
        IHttpBodyControlFeature? syncIoFeature = context.Features.Get<IHttpBodyControlFeature>();
        if (syncIoFeature != null)
        {
            syncIoFeature.AllowSynchronousIO = true;
        }
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? corsDevPolicy : corsProdPolicy);

Anonymous(
    app.MapPost<EncodeTextRequest>("/codec/encode/text"),
    app.MapPost<EncodeBinaryRequest>("/codec/encode/binary"),
    app.MapPost<DecodeRequest>("/codec/decode")
);

app.Run();
