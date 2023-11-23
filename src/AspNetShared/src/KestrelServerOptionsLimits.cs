namespace AspNetShared;

public class KestrelServerOptionsLimits
{
    public const string Section = "Limits";

    public long? MaxRequestBodySize { get; set; }
}
