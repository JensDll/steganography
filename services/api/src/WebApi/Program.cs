using Domain;
using MinimalApiBuilder;
using WebApi.Extensions;
using WebApi.Features.Codec.Decode;
using WebApi.Features.Codec.EncodeBinary;
using WebApi.Features.Codec.EncodeText;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilogLogger();

builder.Configuration.AddDefaultJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMinimalApiBuilderEndpoints();
builder.Services.AddDomain();

builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddDefaultPolicy(
        corsBuilder =>
        {
            corsBuilder.WithOrigins(builder.Configuration.AllowedOrigins());
        });
});

builder.Services.AddHsts(hstsOptions =>
{
    hstsOptions.Preload = true;
    hstsOptions.IncludeSubDomains = true;
    hstsOptions.MaxAge = TimeSpan.FromDays(365);
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
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

RouteGroupBuilder codec = app.MapGroup("/codec").WithTags("Codec");
codec.MapPost<EncodeTextEndpoint>("/encode/text");
codec.MapPost<EncodeBinaryEndpoint>("/encode/binary");
codec.MapPost<DecodeEndpoint>("/decode");

RouteGroupBuilder auxiliary = app.MapGroup("/").WithTags("Auxiliary");
auxiliary.MapGet("/health", () => TypedResults.Ok());

app.Run();
