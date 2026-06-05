using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Pricing.Requests;

/// <summary>Manuel stok ayarlama (hammadde teslim, fire, sayım düzeltmesi).</summary>
public class StockAdjustmentRequest
{
    /// <summary>Pozitif: stoğa ekle. Negatif: stoktan düş.</summary>
    [Range(-1_000_000, 1_000_000)]
    public decimal DeltaKg { get; set; }

    [StringLength(200)]
    public string? Reason { get; set; }
}
