namespace CncMes.Domain.Production;

/// <summary>
/// Bir iş tamamlandığında üretilen maliyet kaleminin türü.
/// Kar/zarar hesabı bu dört kalemden toplanır.
/// </summary>
public static class CostType
{
    /// <summary>Hammadde: tüketilen hacim × birim maliyet.</summary>
    public const string Material = "Material";

    /// <summary>İşçilik: işleme süresi × dakika ücreti.</summary>
    public const string Labor = "Labor";

    /// <summary>Elektrik: süre × makine gücü (kW) × kWh birim fiyatı.</summary>
    public const string Electricity = "Electricity";

    /// <summary>Takım aşınması: süre × dakika başına takım aşınma maliyeti.</summary>
    public const string ToolWear = "ToolWear";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Material, Labor, Electricity, ToolWear,
    };

    public static string ToTurkish(string type) => type switch
    {
        Material    => "Hammadde",
        Labor       => "İşçilik",
        Electricity => "Elektrik",
        ToolWear    => "Takım Aşınması",
        _           => type,
    };
}
