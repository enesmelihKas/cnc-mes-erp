using CncMes.Application.Production;

namespace CncMes.Application.Pricing;

public interface IPricingService
{
    Task<PricingSettingsDto> GetAsync(CancellationToken ct = default);
    Task<PricingSettingsDto> UpdateAsync(PricingSettingsRequest request, CancellationToken ct = default);

    /// <summary>
    /// Geometriye + parça özelliklerine + hammaddeye + ayarlara göre süre, fiyat ve breakdown üretir.
    /// </summary>
    PriceEstimate Estimate(
        StlGeometryDto geometry,
        PartFeaturesSnapshot features,
        MaterialDto material,
        PricingSettingsDto settings);
}

/// <summary>
/// Estimate'in ihtiyaç duyduğu, OrderItem'dan alınan parça özelliklerinin snapshot'u.
/// (PartFeaturesRequest UI-bound; bu salt internal hesap için.)
/// </summary>
public record PartFeaturesSnapshot(
    string ToleranceGrade,
    string SurfaceFinish,
    int HoleCount,
    bool HeatTreated,
    int Quantity
);

public record PriceEstimate(
    decimal EstimatedMinutes,
    decimal EstimatedUnitPrice,
    PriceBreakdown Breakdown
);
