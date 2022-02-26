namespace WebApi.Contracts;

public static class ApiRoutes
{
    private const string _base = "api";

    public static class CodecRoutes
    {
        public const string EncodeText = _base + "/codec/encode/text";

        public const string EncodeBinary = _base + "/codec/encode/binary";
    }
}
