namespace CncMes.Domain.Production;

/// <summary>
/// Bir iş (Job) tamamlandığında otomatik üretilen maliyet kaydı.
/// Her tamamlanan iş için <see cref="CostType"/> başına bir satır oluşur.
/// Kar/zarar raporları bu kayıtları sipariş/ay bazında toplar.
/// </summary>
public class CostEntry
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    /// <summary>Material / Labor / Electricity / ToolWear (bkz. <see cref="CostType"/>).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Bu kalemin TL tutarı.</summary>
    public decimal AmountTL { get; set; }

    /// <summary>Hesabın nasıl çıktığını gösteren insan-okur açıklama (örn. "12.5 cm³ × 2.50 TL × 3").</summary>
    public string? Detail { get; set; }

    public DateTime CreatedAt { get; set; }
}
