namespace AspNetShared;

public class KestrelServerOptions
{
    public const string Section = "Kestrel";

    public KestrelServerOptionsLimits? Limits { get; set; }
}
