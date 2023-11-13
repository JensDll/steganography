using api;
using api.extensions;
using api.features.v1.codec;
using Asp.Versioning.Builder;
using aspnet.common;
using aspnet.common.logging;
using aspnet.common.middleware.development_proxy;
using aspnet.common.middleware.static_compressed_file;
using aspnet.common.options.http_headers;
using aspnet.common.options.kestrel;
using domain;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using MinimalApiBuilder;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDefaultJson();

builder.Logging.AddSerilogLogger();

builder.WebHost.ConfigureKestrel(static (context, serverOptions) =>
{
    context.Configuration.GetRequiredSection(KestrelServerOptions.Section).Bind(serverOptions);
    context.ConfigureCertificate(serverOptions);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMinimalApiBuilderEndpoints();
builder.Services.AddDomain();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.Configure<RouteHandlerOptions>(static options =>
{
    options.ThrowOnBadRequest = false;
});

builder.Services.AddHsts(static hstsOptions =>
{
    hstsOptions.Preload = true;
    hstsOptions.IncludeSubDomains = true;
    hstsOptions.MaxAge = TimeSpan.FromDays(3650);
});

builder.AddHttpHeadersOptions();

StaticCompressedFileOptions staticFileOptions = new()
{
    OnPrepareResponse = StaticCompressedFileOptions.DefaultOnPrepareResponse
};

StaticCompressedFileOptions indexStaticFileOptions = new()
{
    OnPrepareResponse = StaticCompressedFileOptions.IndexOnPrepareResponse
};

builder.Services.AddDevelopmentProxyMiddleware()
    .AddStaticCompressedFileMiddleware(staticFileOptions);

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

RewriteOptions rewriteOptions = new();
rewriteOptions.AddRedirectToNonWww();

app.UseRewriter(rewriteOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStaticCompressedFiles();
app.UseStaticFiles(staticFileOptions);

app.UseStatusCodePages();

app.UseRouting();

IVersionedEndpointRouteBuilder versioned = app.NewVersionedApi();

RouteGroupBuilder api = versioned.MapGroup("/api");

api.MapHealthChecks("/healthz").IsApiVersionNeutral();

RouteGroupBuilder v1 = api.MapGroup("/v{version:apiVersion}").HasApiVersion(1.0);
v1.MapCodecFeature();

if (app.Environment.IsRunningInContainer())
{
    app.MapFallbackToFile("index.html", indexStaticFileOptions);
}

app.UseDevelopmentProxy(new Uri("http://localhost:5173"));

app.Run();
