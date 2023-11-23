using MinimalApiBuilder;

namespace api.features.v1.codec;

internal static class FeatureExtensions
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
