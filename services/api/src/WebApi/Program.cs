using Domain;
using MinimalApiBuilder;
using WebApi.Extensions;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;
using WebApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = "Properties"
});

builder.AddSerilogLogger();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMinimalApiBuilderEndpoints()
    .AddDomain()
    .AddCors(corsOptions =>
    {
        corsOptions.AddDefaultPolicy(
            corsBuilder => { corsBuilder.WithOrigins(builder.Configuration.AllowedOrigins()); });
    });

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    context.Configuration.GetSection("Kestrel:Limits").Bind(serverOptions.Limits);
    context.ConfigureCertificate(serverOptions);
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseMiddleware<LoggingMiddleware>();

RouteGroupBuilder codec = app.MapGroup("/codec").WithTags("Codec");
codec.MapPost<EncodeTextEndpoint>("/encode/text");
codec.MapPost<EncodeBinaryEndpoint>("/encode/binary");
codec.MapPost<DecodeEndpoint>("/decode");

RouteGroupBuilder auxiliary = app.MapGroup("/").WithTags("Auxiliary");
auxiliary.MapGet("/health", () => TypedResults.Ok());

app.Run();
