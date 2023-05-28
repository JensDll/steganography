using MinimalApiBuilder;

namespace steganography.api.features.codec;

public static class Extensions
{
    public static IEndpointRouteBuilder MapCodecFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder codec = endpoints.MapGroup("/codec").WithTags("Codec");
        codec.MapPost<EncodeTextEndpoint>("/encode/text");
        codec.MapPost<EncodeBinaryEndpoint>("/encode/binary");
        codec.MapPost<DecodeEndpoint>("/decode");

        return endpoints;
    }
}
