using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Pricing;

public record PricingSettingsDto(
    // Temel maliyetler
    decimal LaborCostPerMinute,
    decimal MachineHourlyRate,
    decimal BaseSetupCost,
    // Süre katsayıları
    decimal MinutesPerCm3,
    decimal MinutesPerThousandTriangles,
    decimal MinutesPerHole,
    decimal AspectFactorWeight,
    // Tolerans çarpanları
    decimal ToleranceFactorRough,
    decimal ToleranceFactorStandard,
    decimal ToleranceFactorPrecise,
    decimal ToleranceFactorUltraPrecise,
    // Yüzey işlem
    decimal SurfaceFinishCostPerCm2Polished,
    decimal SurfaceFinishCostPerCm2Anodized,
    // Karmaşıklık & ısıl işlem
    decimal ComplexityMultiplier,
    decimal HeatTreatmentMultiplier,
    // Kar/zarar maliyet katsayıları
    decimal ElectricityCostPerKwh,
    decimal MachinePowerKw,
    decimal ToolWearCostPerMinute,
    DateTime UpdatedAt
);

public class PricingSettingsRequest
{
    [Range(0, 100_000)] public decimal LaborCostPerMinute { get; set; }
    [Range(0, 100_000)] public decimal MachineHourlyRate { get; set; }
    [Range(0, 1_000_000)] public decimal BaseSetupCost { get; set; }

    [Range(0, 1000)] public decimal MinutesPerCm3 { get; set; }
    [Range(0, 1000)] public decimal MinutesPerThousandTriangles { get; set; }
    [Range(0, 1000)] public decimal MinutesPerHole { get; set; }
    [Range(0, 10)]   public decimal AspectFactorWeight { get; set; }

    [Range(0.1, 100)] public decimal ToleranceFactorRough { get; set; }
    [Range(0.1, 100)] public decimal ToleranceFactorStandard { get; set; }
    [Range(0.1, 100)] public decimal ToleranceFactorPrecise { get; set; }
    [Range(0.1, 100)] public decimal ToleranceFactorUltraPrecise { get; set; }

    [Range(0, 1000)] public decimal SurfaceFinishCostPerCm2Polished { get; set; }
    [Range(0, 1000)] public decimal SurfaceFinishCostPerCm2Anodized { get; set; }

    [Range(0, 100)] public decimal ComplexityMultiplier { get; set; }
    [Range(0.1, 10)] public decimal HeatTreatmentMultiplier { get; set; }

    [Range(0, 1000)] public decimal ElectricityCostPerKwh { get; set; }
    [Range(0, 1000)] public decimal MachinePowerKw { get; set; }
    [Range(0, 1000)] public decimal ToolWearCostPerMinute { get; set; }
}
