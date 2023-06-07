using aspnet.common;
using aspnet.common.logging;
using aspnet.common.middleware.development_proxy;
using aspnet.common.middleware.static_compressed_file;
using aspnet.common.options.http_headers;
using aspnet.common.options.kestrel;
using Microsoft.AspNetCore.Rewrite;
using MinimalApiBuilder;
using steganography.api.extensions;
using steganography.api.features.codec;
using steganography.domain;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDefaultJson();

builder.Logging.AddSerilogLogger();

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    context.Configuration.GetRequiredSection(KestrelServerOptions.Section).Bind(serverOptions);
    context.ConfigureCertificate(serverOptions);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMinimalApiBuilderEndpoints();
builder.Services.AddDomain();
builder.Services.AddHealthChecks();

builder.Services.AddHsts(hstsOptions =>
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

app.UseStaticCompressedFiles();
app.UseStaticFiles(staticFileOptions);

app.UseRouting();

RouteGroupBuilder api = app.MapGroup("/api");
api.MapCodecFeature();
api.MapHealthChecks("/healthz");

if (app.Environment.IsRunningInContainer())
{
    app.MapFallbackToFile("index.html", indexStaticFileOptions);
}

app.UseDevelopmentProxy("http://localhost:5173");

app.Run();
