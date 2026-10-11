namespace CncMes.Domain.Pricing;

public class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Ham hammaddenin cm³ başına TL maliyeti.</summary>
    public decimal CostPerCm3 { get; set; }

    /// <summary>İşlenebilirlik faktörü: 1.0 referans (alüminyum), çelik ~1.5, paslanmaz ~2.5, plastik ~0.6.</summary>
    public decimal MachinabilityFactor { get; set; }

    /// <summary>Hammaddenin yoğunluğu (g/cm³); stok düşümünde kg hesabı için kullanılır.</summary>
    public decimal DensityGramPerCm3 { get; set; }

    /// <summary>Eldeki hammadde miktarı (kg). Negatif olabilir (aşım uyarısı için).</summary>
    public decimal StockKg { get; set; }

    /// <summary>Bu eşiğin altına düşünce dashboard uyarısı çıkar.</summary>
    public decimal LowStockThreshold { get; set; }

    public bool IsActive { get; set; } = true;
}
