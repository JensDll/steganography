using aspnet.shared;
using aspnet.shared.logging;
using aspnet.shared.middleware.development_proxy;
using aspnet.shared.middleware.static_compressed_file;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using MinimalApiBuilder;
using steganography.api.extensions;
using steganography.api.features.codec;
using steganography.api.options.http_headers;
using steganography.api.options.kestrel;
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

builder.Services.AddDevelopmentProxyMiddleware()
    .AddStaticCompressedFileMiddleware();

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
app.UseStaticFiles();

app.UseRouting();

RouteGroupBuilder api = app.MapGroup("/api");
api.MapCodecFeature();
api.MapHealthChecks("/health");

StaticFileOptions indexHtmlOptions = new StaticFileOptions
{
    OnPrepareResponse = static context =>
    {
        HttpHeadersOptions options =
            context.Context.RequestServices.GetRequiredService<IOptions<HttpHeadersOptions>>().Value;

        IHeaderDictionary headers = context.Context.Response.Headers;
        headers.CacheControl = "private,no-cache,no-store,must-revalidate,max-age=0";
        headers.XXSSProtection = "0";

        if (options.ContentSecurityPolicyPolicy is not null)
        {
            headers.ContentSecurityPolicy = options.ContentSecurityPolicyPolicy;
        }
        else if (options.ContentSecurityPolicyPolicyReportOnly is not null)
        {
            headers.ContentSecurityPolicyReportOnly = options.ContentSecurityPolicyPolicyReportOnly;
        }
    }
};

if (app.Environment.IsRunningInContainer())
{
    app.MapFallbackToFile("index.html", indexHtmlOptions);
}

app.UseDevelopmentProxy("http://localhost:5173");

app.Run();
