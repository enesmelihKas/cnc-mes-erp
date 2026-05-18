namespace CncMes.Domain.Pricing;

public static class SurfaceFinish
{
    public const string AsMilled = "AsMilled";   // İşlenmiş (varsayılan, ek maliyet yok)
    public const string Polished = "Polished";   // Cilalı (cm² başına ücret)
    public const string Anodized = "Anodized";   // Eloksal (cm² başına daha pahalı)

    public static readonly IReadOnlyList<string> All = new[]
    {
        AsMilled, Polished, Anodized
    };

    public static string ToTurkish(string finish) => finish switch
    {
        AsMilled => "İşlenmiş",
        Polished => "Cilalı",
        Anodized => "Eloksal",
        _        => finish,
    };
}
