using Domain;
using Microsoft.AspNetCore.Http.Features;
using WebApi.ModelBinding.Providers;

const string corsDevPolicy = "cors:dev";

WebApplicationBuilder webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services.AddControllers(mvcOptions =>
{
    mvcOptions.ModelBinderProviders.Insert(0, new EncodeRequestModelBinderProvider());
});
webBuilder.Services.AddEndpointsApiExplorer();
webBuilder.Services.AddSwaggerGen();
webBuilder.Services.AddCors(options =>
{
    options.AddPolicy(corsDevPolicy, corsBuilder => { corsBuilder.AllowAnyOrigin(); });
});
webBuilder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 268435456; });
webBuilder.Services.AddDataProtection();
webBuilder.Services.AddDomain();

WebApplication app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(corsDevPolicy);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
