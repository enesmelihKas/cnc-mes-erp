using CncMes.Application.Pricing;
using CncMes.Application.Production;
using CncMes.Domain.Pricing;
using CncMes.Domain.Sales;
using CncMes.Infrastructure.Persistence;
using IxMilia.Stl;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Production;

public class StlAnalysisService : IStlAnalysisService
{
    private readonly AppDbContext _db;
    private readonly IPricingService _pricing;
    private readonly IMaterialService _materials;
    private readonly StlStorageOptions _storage;

    public StlAnalysisService(
        AppDbContext db,
        IPricingService pricing,
        IMaterialService materials,
        StlStorageOptions storage)
    {
        _db = db;
        _pricing = pricing;
        _materials = materials;
        _storage = storage;
    }

    public async Task<StlAnalysisResultDto> UploadAndAnalyzeAsync(
        int orderItemId,
        string originalFileName,
        Stream stlContent,
        CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={orderItemId}).");

        using var buffer = new MemoryStream();
        await stlContent.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        StlFile stl;
        try
        {
            stl = StlFile.Load(buffer);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "STL dosyası okunamadı. Geçerli bir ASCII/Binary STL mi?", ex);
        }

        var geometry = StlGeometryCalculator.Calculate(stl);

        // Eski dosyayı sil, yenisini yaz.
        DeleteIfExists(item.StlFilePath);
        Directory.CreateDirectory(_storage.BaseDirectory);
        var relativeName = $"{item.Id:D6}_{Guid.NewGuid():N}_{SanitizeFileName(originalFileName)}";
        var fullPath = Path.Combine(_storage.BaseDirectory, relativeName);
        buffer.Position = 0;
        await using (var file = File.Create(fullPath))
        {
            await buffer.CopyToAsync(file, ct);
        }

        // OrderItem'a geometri yaz.
        item.StlFileName = originalFileName;
        item.StlFilePath = relativeName;
        WriteGeometry(item, geometry);
        item.AnalyzedAt = DateTime.UtcNow;

        // Tahmin yap (features + material varsa) ve fiyat kaydet.
        var estimate = await TryEstimateAsync(item, geometry, ct);
        if (estimate is not null)
        {
            item.EstimatedMinutes = estimate.EstimatedMinutes;
            item.EstimatedUnitPrice = estimate.EstimatedUnitPrice;
        }

        await _db.SaveChangesAsync(ct);

        return new StlAnalysisResultDto(
            item.Id,
            originalFileName,
            relativeName,
            geometry,
            estimate?.EstimatedMinutes ?? 0,
            estimate?.EstimatedUnitPrice ?? 0,
            item.AnalyzedAt.Value);
    }

    public async Task RecalculateAsync(int orderItemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={orderItemId}).");

        // STL yoksa hesap yapamayız.
        if (item.VolumeCm3 is null)
        {
            return;
        }

        var geometry = ReadGeometry(item);
        var estimate = await TryEstimateAsync(item, geometry, ct);
        if (estimate is null) return;

        item.EstimatedMinutes = estimate.EstimatedMinutes;
        item.EstimatedUnitPrice = estimate.EstimatedUnitPrice;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int orderItemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={orderItemId}).");

        DeleteIfExists(item.StlFilePath);

        item.StlFileName = null;
        item.StlFilePath = null;
        item.VolumeCm3 = null;
        item.SurfaceAreaCm2 = null;
        item.BoundingBoxXCm = null;
        item.BoundingBoxYCm = null;
        item.BoundingBoxZCm = null;
        item.TriangleCount = null;
        item.AspectRatio = null;
        item.Solidity = null;
        item.ExcessMaterialRatio = null;
        item.IsClosedMesh = null;
        item.EstimatedMinutes = null;
        item.EstimatedUnitPrice = null;
        item.AnalyzedAt = null;

        await _db.SaveChangesAsync(ct);
    }

    public async Task ApplyEstimatedPriceAsync(int orderItemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == orderItemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={orderItemId}).");

        if (item.EstimatedUnitPrice is null)
        {
            throw new InvalidOperationException(
                "Tahmini fiyat hesaplanmamış. Önce STL yükleyin ve parça özelliklerini girin.");
        }

        item.UnitPrice = item.EstimatedUnitPrice.Value;
        await _db.SaveChangesAsync(ct);
    }

    // --- helpers ---

    private async Task<PriceEstimate?> TryEstimateAsync(OrderItem item, StlGeometryDto geometry, CancellationToken ct)
    {
        if (item.MaterialId is null) return null;

        var material = await _materials.GetAsync(item.MaterialId.Value, ct);
        if (material is null) return null;

        var settings = await _pricing.GetAsync(ct);

        var features = new PartFeaturesSnapshot(
            ToleranceGrade: item.ToleranceGrade,
            SurfaceFinish: item.SurfaceFinish,
            HoleCount: item.HoleCount,
            HeatTreated: item.HeatTreated,
            Quantity: item.Quantity);

        return _pricing.Estimate(geometry, features, material, settings);
    }

    private static void WriteGeometry(OrderItem item, StlGeometryDto g)
    {
        item.VolumeCm3 = g.VolumeCm3;
        item.SurfaceAreaCm2 = g.SurfaceAreaCm2;
        item.BoundingBoxXCm = g.BoundingBoxXCm;
        item.BoundingBoxYCm = g.BoundingBoxYCm;
        item.BoundingBoxZCm = g.BoundingBoxZCm;
        item.TriangleCount = g.TriangleCount;
        item.AspectRatio = g.AspectRatio;
        item.Solidity = g.Solidity;
        item.ExcessMaterialRatio = g.ExcessMaterialRatio;
        item.IsClosedMesh = g.IsClosedMesh;
    }

    private static StlGeometryDto ReadGeometry(OrderItem item) => new(
        VolumeCm3:           item.VolumeCm3 ?? 0,
        SurfaceAreaCm2:      item.SurfaceAreaCm2 ?? 0,
        BoundingBoxXCm:      item.BoundingBoxXCm ?? 0,
        BoundingBoxYCm:      item.BoundingBoxYCm ?? 0,
        BoundingBoxZCm:      item.BoundingBoxZCm ?? 0,
        TriangleCount:       item.TriangleCount ?? 0,
        AspectRatio:         item.AspectRatio ?? 1,
        Solidity:            item.Solidity ?? 1,
        ExcessMaterialRatio: item.ExcessMaterialRatio ?? 1,
        IsClosedMesh:        item.IsClosedMesh ?? false);

    private void DeleteIfExists(string? relativeName)
    {
        if (string.IsNullOrWhiteSpace(relativeName)) return;
        var full = Path.Combine(_storage.BaseDirectory, relativeName);
        if (File.Exists(full))
        {
            try { File.Delete(full); } catch { /* dosya kilitliyse sessiz geç */ }
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        if (clean.Length > 80) clean = clean.Substring(0, 80);
        return clean;
    }
}
