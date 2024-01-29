namespace Steganography.Common;

internal sealed class KestrelOptions
{
    public const string Section = "Kestrel";

    public KestrelOptionsLimits? Limits { get; set; }
}
