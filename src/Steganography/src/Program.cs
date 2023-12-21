using Domain;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using MinimalApiBuilder.Generator;
using MinimalApiBuilder.Middleware;
using Steganography.Features.Codec;
using static MinimalApiBuilder.Generator.ConfigureEndpoints;
#if NOT_RUNNING_IN_CONTAINER
using Steganography.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile(Path.Join("Properties", "appSettings.json"), false, false);

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

CompressedStaticFileOptions staticFileOptions = new()
{
    OnPrepareResponse = static context =>
    {
        IHeaderDictionary headers = context.Context.Response.Headers;

        headers.XContentTypeOptions = Headers.NoSniff;

        if (context.Filename.EndsWith(".html", StringComparison.Ordinal))
        {
            headers.CacheControl = Headers.CacheControlHtml;
            headers.XXSSProtection = Headers.XXSSProtection;
            return;
        }

        headers.CacheControl = Headers.CacheControl;
    },
    ContentTypeProvider = ContentTypeProvider.Instance
};

builder.Services.AddCompressedStaticFileMiddleware(staticFileOptions);

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

app.UseCompressedStaticFiles();

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
}

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

app.MapFallbackToIndexHtml();

#if NOT_RUNNING_IN_CONTAINER
app.UseSwagger();
app.UseSwaggerUI();
#endif

app.Run();
