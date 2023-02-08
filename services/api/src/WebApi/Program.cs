using Domain;
using MinimalApiBuilder;
using WebApi.Extensions;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = "Properties"
});

builder.AddSerilogLogger();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMinimalApiBuilderEndpoints();
builder.Services.AddDomain();

builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddDefaultPolicy(
        corsBuilder => { corsBuilder.WithOrigins(builder.Configuration.AllowedOrigins()); });
});

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    context.ConfigureCertificate(serverOptions);
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

RouteGroupBuilder codec = app.MapGroup("/codec").WithTags("Codec");
codec.MapPost<EncodeTextEndpoint>("/encode/text").RequireHost();
codec.MapPost<EncodeBinaryEndpoint>("/encode/binary");
codec.MapPost<DecodeEndpoint>("/decode");

RouteGroupBuilder auxiliary = app.MapGroup("/").WithTags("Auxiliary");
auxiliary.MapGet("/health", () => TypedResults.Ok());

app.Run();
