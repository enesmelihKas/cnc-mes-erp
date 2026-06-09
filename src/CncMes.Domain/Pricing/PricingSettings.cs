namespace CncMes.Domain.Pricing;

public class PricingSettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;

    // --- Temel maliyetler ---
    public decimal LaborCostPerMinute { get; set; }
    public decimal MachineHourlyRate { get; set; }
    public decimal BaseSetupCost { get; set; }

    // --- Süre katsayıları ---
    public decimal MinutesPerCm3 { get; set; }
    public decimal MinutesPerThousandTriangles { get; set; }
    public decimal MinutesPerHole { get; set; }
    public decimal AspectFactorWeight { get; set; }

    // --- Tolerans çarpanları ---
    public decimal ToleranceFactorRough { get; set; }
    public decimal ToleranceFactorStandard { get; set; }
    public decimal ToleranceFactorPrecise { get; set; }
    public decimal ToleranceFactorUltraPrecise { get; set; }

    // --- Yüzey işlem (TL/cm²) ---
    public decimal SurfaceFinishCostPerCm2Polished { get; set; }
    public decimal SurfaceFinishCostPerCm2Anodized { get; set; }

    // --- Karmaşıklık & Isıl işlem ---
    public decimal ComplexityMultiplier { get; set; }
    public decimal HeatTreatmentMultiplier { get; set; }

    // --- Kar/Zarar maliyet katsayıları (Phase 6) ---
    /// <summary>Elektrik birim fiyatı (TL/kWh).</summary>
    public decimal ElectricityCostPerKwh { get; set; }
    /// <summary>Tezgâhın çektiği ortalama güç (kW) — elektrik maliyeti için.</summary>
    public decimal MachinePowerKw { get; set; }
    /// <summary>Dakika başına takım aşınma maliyeti (TL/dk) — sabit yaklaşım.</summary>
    public decimal ToolWearCostPerMinute { get; set; }

    public DateTime UpdatedAt { get; set; }
}
