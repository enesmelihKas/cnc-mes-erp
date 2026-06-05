using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Pricing.Requests;

public class MaterialRequest
{
    [Required(ErrorMessage = "Hammadde adı zorunlu.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.0001, 1_000_000, ErrorMessage = "Birim maliyet pozitif olmalı.")]
    public decimal CostPerCm3 { get; set; }

    [Range(0.01, 100, ErrorMessage = "Machinability faktörü 0.01–100 arası olmalı.")]
    public decimal MachinabilityFactor { get; set; } = 1.0m;

    [Range(0.01, 100, ErrorMessage = "Yoğunluk 0.01–100 g/cm³ arası olmalı.")]
    public decimal DensityGramPerCm3 { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Stok negatif olamaz.")]
    public decimal StockKg { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Eşik negatif olamaz.")]
    public decimal LowStockThreshold { get; set; }

    public bool IsActive { get; set; } = true;
}
