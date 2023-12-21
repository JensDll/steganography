#if NOT_RUNNING_IN_CONTAINER
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steganography.Common;

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
