using CncMes.Application.Pricing;
using CncMes.Application.Production;
using CncMes.Domain.Pricing;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Pricing;

public class PricingService : IPricingService
{
    private readonly AppDbContext _db;

    public PricingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PricingSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await _db.PricingSettings.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == PricingSettings.SingletonId, ct)
            ?? throw new InvalidOperationException(
                "Fiyatlandırma ayarları seed edilmemiş. DbInitializer'ı kontrol et.");

        return ToDto(settings);
    }

    public async Task<PricingSettingsDto> UpdateAsync(PricingSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await _db.PricingSettings
            .FirstOrDefaultAsync(p => p.Id == PricingSettings.SingletonId, ct)
            ?? throw new InvalidOperationException("Fiyatlandırma ayarları seed edilmemiş.");

        settings.LaborCostPerMinute = request.LaborCostPerMinute;
        settings.MachineHourlyRate = request.MachineHourlyRate;
        settings.BaseSetupCost = request.BaseSetupCost;
        settings.MinutesPerCm3 = request.MinutesPerCm3;
        settings.MinutesPerThousandTriangles = request.MinutesPerThousandTriangles;
        settings.MinutesPerHole = request.MinutesPerHole;
        settings.AspectFactorWeight = request.AspectFactorWeight;
        settings.ToleranceFactorRough = request.ToleranceFactorRough;
        settings.ToleranceFactorStandard = request.ToleranceFactorStandard;
        settings.ToleranceFactorPrecise = request.ToleranceFactorPrecise;
        settings.ToleranceFactorUltraPrecise = request.ToleranceFactorUltraPrecise;
        settings.SurfaceFinishCostPerCm2Polished = request.SurfaceFinishCostPerCm2Polished;
        settings.SurfaceFinishCostPerCm2Anodized = request.SurfaceFinishCostPerCm2Anodized;
        settings.ComplexityMultiplier = request.ComplexityMultiplier;
        settings.HeatTreatmentMultiplier = request.HeatTreatmentMultiplier;
        settings.ElectricityCostPerKwh = request.ElectricityCostPerKwh;
        settings.MachinePowerKw = request.MachinePowerKw;
        settings.ToolWearCostPerMinute = request.ToolWearCostPerMinute;
        settings.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(settings);
    }

    public PriceEstimate Estimate(
        StlGeometryDto g,
        PartFeaturesSnapshot f,
        MaterialDto material,
        PricingSettingsDto s)
    {
        // --- Hammadde ---
        // waste_factor: dış BBox hacmi / parça hacmi (1.0-3.0 arası clamp). Yüksek = çok talaş.
        var bboxVolume = g.BoundingBoxXCm * g.BoundingBoxYCm * g.BoundingBoxZCm;
        var wasteFactor = g.VolumeCm3 > 0
            ? Math.Clamp(bboxVolume / g.VolumeCm3, 1.0m, 3.0m)
            : 1.0m;
        var materialCost = g.VolumeCm3 * material.CostPerCm3 * wasteFactor;

        // --- Süre tahmini (raw) ---
        var minutesBase = g.VolumeCm3 * s.MinutesPerCm3;
        var minutesComplexity = (g.TriangleCount / 1000m) * s.MinutesPerThousandTriangles;
        var minutesHoles = f.HoleCount * s.MinutesPerHole;
        var rawMinutes = minutesBase + minutesComplexity + minutesHoles;

        // --- Süre çarpanları ---
        var machinabilityFactor = material.MachinabilityFactor;
        var toleranceFactor = ResolveToleranceFactor(f.ToleranceGrade, s);
        var solidityFactor = ResolveSolidityFactor(g.Solidity);
        var aspectFactor = ResolveAspectFactor(g.AspectRatio, s.AspectFactorWeight);

        var effectiveMinutes = rawMinutes * machinabilityFactor * toleranceFactor * solidityFactor;

        // --- İşçilik & makine ---
        var laborCost = effectiveMinutes * s.LaborCostPerMinute;
        var machineCost = effectiveMinutes * (s.MachineHourlyRate / 60m);

        // --- Kurulum (adet amortizasyonu) ---
        var qty = Math.Max(f.Quantity, 1);
        var setupCost = (s.BaseSetupCost * s.ComplexityMultiplier * aspectFactor) / qty;

        // --- Yüzey işlem (cm² lineer) ---
        var finishUnit = ResolveSurfaceFinishUnitCost(f.SurfaceFinish, s);
        var finishCost = finishUnit * g.SurfaceAreaCm2;

        // --- Toplam (subtotal × heat factor) ---
        var subtotal = materialCost + laborCost + machineCost + setupCost + finishCost;
        var heatFactor = f.HeatTreated ? s.HeatTreatmentMultiplier : 1.0m;
        var total = subtotal * heatFactor;
        var heatDelta = total - subtotal;

        // --- Yuvarla ---
        materialCost = Math.Round(materialCost, 2);
        laborCost = Math.Round(laborCost, 2);
        machineCost = Math.Round(machineCost, 2);
        setupCost = Math.Round(setupCost, 2);
        finishCost = Math.Round(finishCost, 2);
        heatDelta = Math.Round(heatDelta, 2);
        subtotal = Math.Round(subtotal, 2);
        total = Math.Round(total, 2);
        var minutes = Math.Round(effectiveMinutes, 2);

        var breakdown = new PriceBreakdown(
            MaterialCost: materialCost,
            LaborCost: laborCost,
            MachineCost: machineCost,
            SetupCost: setupCost,
            FinishCost: finishCost,
            HeatTreatmentDelta: heatDelta,
            Subtotal: subtotal,
            Total: total,
            RawMinutes: Math.Round(rawMinutes, 2),
            EffectiveMinutes: minutes,
            WasteFactor: Math.Round(wasteFactor, 3),
            MachinabilityFactor: machinabilityFactor,
            ToleranceFactor: toleranceFactor,
            SolidityFactor: Math.Round(solidityFactor, 3),
            AspectFactor: Math.Round(aspectFactor, 3),
            MaterialName: material.Name,
            ToleranceLabel: ToleranceGrade.ToTurkish(f.ToleranceGrade),
            SurfaceFinishLabel: SurfaceFinish.ToTurkish(f.SurfaceFinish));

        return new PriceEstimate(minutes, total, breakdown);
    }

    private static decimal ResolveToleranceFactor(string grade, PricingSettingsDto s) => grade switch
    {
        ToleranceGrade.Rough        => s.ToleranceFactorRough,
        ToleranceGrade.Standard     => s.ToleranceFactorStandard,
        ToleranceGrade.Precise      => s.ToleranceFactorPrecise,
        ToleranceGrade.UltraPrecise => s.ToleranceFactorUltraPrecise,
        _                           => s.ToleranceFactorStandard,
    };

    private static decimal ResolveSurfaceFinishUnitCost(string finish, PricingSettingsDto s) => finish switch
    {
        SurfaceFinish.Polished => s.SurfaceFinishCostPerCm2Polished,
        SurfaceFinish.Anodized => s.SurfaceFinishCostPerCm2Anodized,
        _                      => 0m,  // AsMilled
    };

    private static decimal ResolveSolidityFactor(decimal solidity)
    {
        // Solidity (0,1] arası. 1.0 = blok parça (factor 1.0). 0.5 = yarım dolu (factor 1.25).
        // clamp [1.0, 2.0]
        var f = 1m + (1m - solidity) / 2m;
        return Math.Clamp(f, 1.0m, 2.0m);
    }

    private static decimal ResolveAspectFactor(decimal aspectRatio, decimal weight)
    {
        // 1 + log10(aspect+1) × weight. aspect=1 → 1 + 0.30×w; aspect=20 → 1 + 1.32×w.
        // decimal log10 yok → double üzerinden hesapla, sonra decimal'a dön.
        var log = (decimal)Math.Log10((double)aspectRatio + 1d);
        return 1m + log * weight;
    }

    private static PricingSettingsDto ToDto(PricingSettings p) => new(
        p.LaborCostPerMinute,
        p.MachineHourlyRate,
        p.BaseSetupCost,
        p.MinutesPerCm3,
        p.MinutesPerThousandTriangles,
        p.MinutesPerHole,
        p.AspectFactorWeight,
        p.ToleranceFactorRough,
        p.ToleranceFactorStandard,
        p.ToleranceFactorPrecise,
        p.ToleranceFactorUltraPrecise,
        p.SurfaceFinishCostPerCm2Polished,
        p.SurfaceFinishCostPerCm2Anodized,
        p.ComplexityMultiplier,
        p.HeatTreatmentMultiplier,
        p.ElectricityCostPerKwh,
        p.MachinePowerKw,
        p.ToolWearCostPerMinute,
        p.UpdatedAt);
}
