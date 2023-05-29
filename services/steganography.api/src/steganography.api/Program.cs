using aspnet.shared;
using aspnet.shared.logging;
using aspnet.shared.middleware.development_proxy;
using aspnet.shared.middleware.static_compressed_file;
using aspnet.shared.options.http_headers;
using aspnet.shared.options.kestrel;
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

builder.Services.AddDevelopmentProxyMiddleware()
    .AddStaticCompressedFileMiddleware("/assets");

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

StaticFileOptions staticFileOptions = new()
{
    OnPrepareResponse = static context =>
    {
        StaticCompressedFileOptions.AddCacheBustingHeaders(context.Context.Response);
    }
};

app.UseStaticCompressedFiles();
app.UseStaticFiles(staticFileOptions);

app.UseRouting();

RouteGroupBuilder api = app.MapGroup("/api");
api.MapCodecFeature();
api.MapHealthChecks("/health");

StaticFileOptions indexHtmlOptions = new()
{
    OnPrepareResponse = static context =>
    {
        StaticCompressedFileOptions.AddIndexHtmlHeaders(context.Context);
    }
};

if (app.Environment.IsRunningInContainer())
{
    app.MapFallbackToFile("index.html", indexHtmlOptions);
}

app.UseDevelopmentProxy("http://localhost:5173");

app.Run();
