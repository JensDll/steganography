namespace Steganography.Common;

internal sealed class KestrelOptionsLimits
{
    public const string Section = "Limits";

    public long? MaxRequestBodySize { get; set; }
}
