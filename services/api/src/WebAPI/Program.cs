using Microsoft.AspNetCore.Http.Features;

string corsDevPolicy = "cors:dev";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => { options.AddPolicy(corsDevPolicy, builder => { builder.AllowAnyOrigin(); }); });
builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 268435456; });

WebApplication app = builder.Build();

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
