namespace CncMes.Domain.Sales;

public static class OrderStatus
{
    public const string Draft = "Draft";
    public const string Confirmed = "Confirmed";
    public const string InProduction = "InProduction";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Draft, Confirmed, InProduction, Completed, Cancelled
    };

    public static string ToTurkish(string status) => status switch
    {
        Draft        => "Taslak",
        Confirmed    => "Onaylandı",
        InProduction => "Üretimde",
        Completed    => "Tamamlandı",
        Cancelled    => "İptal",
        _            => status,
    };
}
