using Domain.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using MiniApi;
using SixLabors.ImageSharp.Formats.Png;
using WebApi.Endpoints.Codec.Request;

namespace WebApi.Endpoints.Codec.Endpoint;

public class EncodeTextEndpoint : Endpoint<EncodeTextRequest>
{
    private readonly IEncoder _encoder;
    private readonly IKeyGenerator _keyGenerator;
    private readonly IDataProtectionProvider _protectionProvider;


    public EncodeTextEndpoint(IEncoder encoder, IKeyGenerator keyGenerator, IDataProtectionProvider protectionProvider)
    {
        _encoder = encoder;
        _keyGenerator = keyGenerator;
        _protectionProvider = protectionProvider;
    }

    public override async Task HandleAsync(EncodeTextRequest request)
    {
        HttpContext.Response.ContentType = "image/png";
        await request.CoverImage.SaveAsync(HttpContext.Response.Body, new PngEncoder());
        request.CoverImage.Dispose();
    }
}
