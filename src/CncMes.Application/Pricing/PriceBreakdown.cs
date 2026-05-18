namespace CncMes.Application.Pricing;

/// <summary>
/// Bir parça için hesaplanmış fiyatın her satır kalemi.
/// UI breakdown gösteriminde her satır ayrı gösterilir.
/// </summary>
public record PriceBreakdown(
    decimal MaterialCost,
    decimal LaborCost,
    decimal MachineCost,
    decimal SetupCost,
    decimal FinishCost,
    decimal HeatTreatmentDelta,
    decimal Subtotal,
    decimal Total,

    // Süre
    decimal RawMinutes,
    decimal EffectiveMinutes,

    // Faktörler (gösterim için)
    decimal WasteFactor,
    decimal MachinabilityFactor,
    decimal ToleranceFactor,
    decimal SolidityFactor,
    decimal AspectFactor,

    // Etiket (UI'da gösterim için)
    string MaterialName,
    string ToleranceLabel,
    string SurfaceFinishLabel
);
