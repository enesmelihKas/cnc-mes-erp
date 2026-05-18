namespace CncMes.Domain.Pricing;

public static class ToleranceGrade
{
    public const string Rough = "Rough";                 // ±0.5 mm
    public const string Standard = "Standard";           // ±0.1 mm
    public const string Precise = "Precise";             // ±0.05 mm
    public const string UltraPrecise = "UltraPrecise";   // ±0.01 mm

    public static readonly IReadOnlyList<string> All = new[]
    {
        Rough, Standard, Precise, UltraPrecise
    };

    public static string ToTurkish(string grade) => grade switch
    {
        Rough        => "Kaba (±0.5 mm)",
        Standard     => "Standart (±0.1 mm)",
        Precise      => "Hassas (±0.05 mm)",
        UltraPrecise => "Ultra Hassas (±0.01 mm)",
        _            => grade,
    };
}
