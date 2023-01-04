using Domain;
using MinimalApiBuilder;
using WebApi.Extensions;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;

const string corsDevelopment = "cors:dev";
const string corsProduction = "cors:prod";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogger();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMinimalApiBuilderEndpoints()
    .AddDomain()
    .AddCors(options =>
    {
        options.AddPolicy(corsDevelopment, corsBuilder => { corsBuilder.AllowAnyOrigin(); });
        options.AddPolicy(corsProduction,
            corsBuilder => { corsBuilder.WithOrigins(builder.Configuration.AllowedOrigins()); });
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 60 * 1024 * 1024; // 60 MB
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(corsDevelopment);
}
else
{
    app.UseCors(corsProduction);
}

RouteGroupBuilder codec = app.MapGroup("/codec").WithTags("Codec");
codec.MapPost<EncodeTextEndpoint>("/encode/text");
codec.MapPost<EncodeBinaryEndpoint>("/encode/binary");
codec.MapPost<DecodeEndpoint>("/decode");

RouteGroupBuilder auxiliary = app.MapGroup("/").WithTags("Auxiliary");
auxiliary.MapGet("/health", () => TypedResults.Ok());

app.Run();
