using CncMes.Application.Pricing;
using CncMes.Application.Pricing.Requests;
using CncMes.Domain.Pricing;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Pricing;

public class MaterialService : IMaterialService
{
    private readonly AppDbContext _db;

    public MaterialService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MaterialDto>> ListAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.Materials.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query
            .OrderBy(m => m.Name)
            .Select(m => new MaterialDto(
                m.Id, m.Name, m.CostPerCm3, m.MachinabilityFactor, m.DensityGramPerCm3,
                m.StockKg, m.LowStockThreshold, m.IsActive))
            .ToListAsync(ct);
    }

    public async Task<MaterialDto?> GetAsync(int id, CancellationToken ct = default)
    {
        return await _db.Materials
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MaterialDto(
                m.Id, m.Name, m.CostPerCm3, m.MachinabilityFactor, m.DensityGramPerCm3,
                m.StockKg, m.LowStockThreshold, m.IsActive))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<MaterialDto> CreateAsync(MaterialRequest request, CancellationToken ct = default)
    {
        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("Hammadde adı boş olamaz.");
        }

        if (await _db.Materials.AnyAsync(m => m.Name == name, ct))
        {
            throw new InvalidOperationException($"'{name}' adlı hammadde zaten kayıtlı.");
        }

        var material = new Material
        {
            Name = name,
            CostPerCm3 = request.CostPerCm3,
            MachinabilityFactor = request.MachinabilityFactor,
            DensityGramPerCm3 = request.DensityGramPerCm3,
            StockKg = request.StockKg,
            LowStockThreshold = request.LowStockThreshold,
            IsActive = request.IsActive,
        };

        _db.Materials.Add(material);
        await _db.SaveChangesAsync(ct);

        return ToDto(material);
    }

    public async Task<MaterialDto> UpdateAsync(int id, MaterialRequest request, CancellationToken ct = default)
    {
        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Hammadde bulunamadı (Id={id}).");

        var name = (request.Name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("Hammadde adı boş olamaz.");
        }

        if (await _db.Materials.AnyAsync(m => m.Id != id && m.Name == name, ct))
        {
            throw new InvalidOperationException($"'{name}' adlı başka bir hammadde zaten var.");
        }

        material.Name = name;
        material.CostPerCm3 = request.CostPerCm3;
        material.MachinabilityFactor = request.MachinabilityFactor;
        material.DensityGramPerCm3 = request.DensityGramPerCm3;
        material.StockKg = request.StockKg;
        material.LowStockThreshold = request.LowStockThreshold;
        material.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);
        return ToDto(material);
    }

    public async Task<MaterialDto> AdjustStockAsync(int id, StockAdjustmentRequest request, CancellationToken ct = default)
    {
        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Hammadde bulunamadı (Id={id}).");

        if (request.DeltaKg == 0)
        {
            return ToDto(material);
        }

        material.StockKg += request.DeltaKg;
        await _db.SaveChangesAsync(ct);
        return ToDto(material);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Hammadde bulunamadı (Id={id}).");

        var usedByItem = await _db.OrderItems.AnyAsync(i => i.MaterialId == id, ct);
        if (usedByItem)
        {
            // Veri bütünlüğü için fiziksel silme yerine pasifleştir.
            material.IsActive = false;
            await _db.SaveChangesAsync(ct);
            return;
        }

        _db.Materials.Remove(material);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MaterialDto>> ListLowStockAsync(CancellationToken ct = default)
    {
        return await _db.Materials
            .AsNoTracking()
            .Where(m => m.IsActive && m.StockKg < m.LowStockThreshold)
            .OrderBy(m => m.StockKg - m.LowStockThreshold)
            .Select(m => new MaterialDto(
                m.Id, m.Name, m.CostPerCm3, m.MachinabilityFactor, m.DensityGramPerCm3,
                m.StockKg, m.LowStockThreshold, m.IsActive))
            .ToListAsync(ct);
    }

    private static MaterialDto ToDto(Material m) => new(
        m.Id, m.Name, m.CostPerCm3, m.MachinabilityFactor, m.DensityGramPerCm3,
        m.StockKg, m.LowStockThreshold, m.IsActive);
}
