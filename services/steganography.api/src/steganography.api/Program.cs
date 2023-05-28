using aspnet.shared;
using aspnet.shared.logging;
using aspnet.shared.middleware.development_proxy;
using aspnet.shared.middleware.static_compressed_file;
using MinimalApiBuilder;
using steganography.api.features.codec;
using steganography.domain;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDefaultJson();

builder.Logging.AddSerilogLogger();

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    context.Configuration.GetSection("Kestrel:Limits").Bind(serverOptions.Limits);
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
    hstsOptions.MaxAge = TimeSpan.FromDays(365);
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

app.UseStaticCompressedFiles();
app.UseStaticFiles();

app.UseRouting();

RouteGroupBuilder api = app.MapGroup("/api");
api.MapCodecFeature();
api.MapHealthChecks("/health");

app.UseDevelopmentProxy();

app.Run();
