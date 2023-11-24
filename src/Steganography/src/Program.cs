using AspNetShared;
using AspNetShared.Middleware.DevelopmentProxy;
using AspNetShared.Middleware.StaticCompressedFile;
using Domain;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using MinimalApiBuilder;
using Steganography.Features.Codec;
using static MinimalApiBuilder.ConfigureEndpoints;
#if NOT_RUNNING_IN_CONTAINER
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile(Path.Join("Properties", "appSettings.json"), false, false);

builder.ConfigureKestrel();
builder.AddHttpHeadersOptions();

builder.Services.AddMinimalApiBuilderEndpoints();
builder.Services.AddDomain();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

#if NOT_RUNNING_IN_CONTAINER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
#endif

builder.Services.Configure<RouteHandlerOptions>(static options =>
{
    options.ThrowOnBadRequest = false;
});

#if RUNNING_IN_CONTAINER
builder.Services.AddHsts(static hstsOptions =>
{
    hstsOptions.Preload = true;
    hstsOptions.IncludeSubDomains = true;
    hstsOptions.MaxAge = TimeSpan.FromDays(3650);
});
#endif

StaticCompressedFileOptions staticFileOptions = new()
{
    OnPrepareResponse = StaticCompressedFileOptions.DefaultOnPrepareResponse
};

#if RUNNING_IN_CONTAINER
StaticCompressedFileOptions indexStaticFileOptions = new()
{
    OnPrepareResponse = StaticCompressedFileOptions.IndexOnPrepareResponse
};
#endif

#if NOT_RUNNING_IN_CONTAINER
builder.Services.AddDevelopmentProxyMiddleware();
#endif
builder.Services.AddStaticCompressedFileMiddleware(staticFileOptions);

WebApplication app = builder.Build();

#if RUNNING_IN_CONTAINER
app.UseHsts();
#endif

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

RewriteOptions rewriteOptions = new();
rewriteOptions.AddRedirectToNonWww();

app.UseRewriter(rewriteOptions);

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
}

app.UseStaticCompressedFiles();
app.UseStaticFiles(staticFileOptions);

app.UseStatusCodePages();

app.UseRouting();

#pragma warning disable IL2026
#pragma warning disable IL3050

RouteGroupBuilder api = app.MapGroup("/api");
api.MapHealthChecks("/healthz");

RouteGroupBuilder v1 = api.MapGroup("/v1");
RouteGroupBuilder codec = v1.MapGroup("/codec").WithTags("Codec");
Configure(
    codec.MapPost("/encode/text", EncodeTextEndpoint.Handle),
    codec.MapPost("/encode/binary", EncodeBinaryEndpoint.Handle),
    codec.MapPost("/decode", DecodeEndpoint.Handle));

RouteGroupBuilder v2 = api.MapGroup("/v2");
v2.MapGet("/env", static (IWebHostEnvironment env) => $"The current environment is: {env.EnvironmentName}");

#if NOT_RUNNING_IN_CONTAINER
app.UseSwagger();
app.UseSwaggerUI();
#endif

#if RUNNING_IN_CONTAINER
app.MapFallbackToFile("index.html", indexStaticFileOptions);
#endif

#if NOT_RUNNING_IN_CONTAINER
app.UseDevelopmentProxy(new Uri("http://localhost:5173"));
#endif

app.Run();

#if NOT_RUNNING_IN_CONTAINER
internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", CreateInfoForApiVersion("v1"));
        options.SwaggerDoc("v2", CreateInfoForApiVersion("v2"));
    }

    private static OpenApiInfo CreateInfoForApiVersion(string version)
    {
        OpenApiInfo info = new()
        {
            Title = "Steganography API",
            Version = version,
            Description = "An image steganography API used to embed encrypted information in cover images."
        };

        return info;
    }
}
#endif
