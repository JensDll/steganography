using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace api;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        StringBuilder descriptionTBuilder =
            new("An image steganography API used to embed encrypted information in cover images.");

        OpenApiInfo info = new()
        {
            Title = "Steganography API",
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact { Name = "Jens Döllmann", Email = "jens@doellmann.com" }
        };

        if (description.IsDeprecated)
        {
            descriptionTBuilder.Append(" This API version has been deprecated.");
        }

        info.Description = descriptionTBuilder.ToString();

        return info;
    }
}
