namespace Contracts;

public static class ApiRoutes
{
    private const string Base = "api";

    public static class CodecRoutes
    {
        public const string EncodeText = Base + "/codec/encode/text";

        public const string EncodeBinary = Base + "/codec/encode/binary";
    }
}
