using CncMes.Application.Reporting;
using CncMes.Domain.Production;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Reporting;

/// <summary>
/// Kar/zarar raporlaması. SQLite + EF Core decimal <c>Sum</c>'ı server-side
/// çeviremediğinden tüm toplamlar veriler materialize edildikten sonra C#
/// tarafında yapılır (proje genel kuralı).
/// </summary>
public class ProfitLossService : IProfitLossService
{
    private readonly AppDbContext _db;

    public ProfitLossService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<OrderProfitLossDto>> ListOrdersAsync(CancellationToken ct = default)
    {
        var list = await BuildAllAsync(ct);
        return list
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .ToList();
    }

    public async Task<OrderProfitLossDto?> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        var list = await BuildAllAsync(ct, orderId);
        return list.FirstOrDefault(o => o.OrderId == orderId);
    }

    public async Task<IReadOnlyList<OrderCostLineDto>> ListOrderCostLinesAsync(int orderId, CancellationToken ct = default)
    {
        // O siparişin işleri (parça adıyla birlikte).
        var jobs = await _db.Jobs.AsNoTracking()
            .Where(j => j.OrderItem!.OrderId == orderId)
            .Select(j => new { j.Id, PartName = j.OrderItem!.PartName })
            .ToListAsync(ct);

        if (jobs.Count == 0) return Array.Empty<OrderCostLineDto>();

        var jobIds = jobs.Select(j => j.Id).ToList();
        var partByJob = jobs.ToDictionary(j => j.Id, j => j.PartName);

        var costs = await _db.CostEntries.AsNoTracking()
            .Where(c => jobIds.Contains(c.JobId))
            .Select(c => new { c.JobId, c.Type, c.AmountTL, c.Detail })
            .ToListAsync(ct);

        return costs
            .Select(c => new OrderCostLineDto(
                c.JobId,
                partByJob.TryGetValue(c.JobId, out var name) ? name : $"İş #{c.JobId}",
                c.Type,
                CostType.ToTurkish(c.Type),
                c.AmountTL,
                c.Detail))
            .OrderBy(l => l.PartName)
            .ThenBy(l => Array.IndexOf(CostType.All.ToArray(), l.Type))
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlyProfitLossDto>> GetMonthlySummaryAsync(CancellationToken ct = default)
    {
        var orders = await BuildAllAsync(ct);

        return orders
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new MonthlyProfitLossDto(
                g.Key.Year,
                g.Key.Month,
                Revenue: g.Sum(o => o.Revenue),
                TotalCost: g.Sum(o => o.TotalCost),
                OrderCount: g.Count()))
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();
    }

    /// <summary>
    /// Tüm siparişler (veya tek sipariş) için gelir + 4 kalem maliyet toplamını
    /// C# tarafında hesaplar. Üç sorgu: orders+items, jobs, cost entries.
    /// </summary>
    private async Task<List<OrderProfitLossDto>> BuildAllAsync(CancellationToken ct, int? onlyOrderId = null)
    {
        var ordersQuery = _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (onlyOrderId is int oid)
        {
            ordersQuery = ordersQuery.Where(o => o.Id == oid);
        }

        var orders = await ordersQuery.ToListAsync(ct);
        if (orders.Count == 0) return new List<OrderProfitLossDto>();

        var orderIds = orders.Select(o => o.Id).ToHashSet();

        // İş → sipariş eşlemesi + durum (tamamlanan iş sayısı için).
        var jobs = await _db.Jobs.AsNoTracking()
            .Where(j => orderIds.Contains(j.OrderItem!.OrderId))
            .Select(j => new { j.Id, j.Status, OrderId = j.OrderItem!.OrderId })
            .ToListAsync(ct);

        var orderByJob = jobs.ToDictionary(j => j.Id, j => j.OrderId);
        var jobIds = jobs.Select(j => j.Id).ToList();

        // Maliyet kalemleri.
        var costs = jobIds.Count == 0
            ? new List<CostRow>()
            : await _db.CostEntries.AsNoTracking()
                .Where(c => jobIds.Contains(c.JobId))
                .Select(c => new CostRow(c.JobId, c.Type, c.AmountTL))
                .ToListAsync(ct);

        // Sipariş başına tür kırılımı (C# tarafında topla).
        var costByOrder = new Dictionary<int, decimal[]>(); // [Material, Labor, Electricity, ToolWear]
        foreach (var c in costs)
        {
            if (!orderByJob.TryGetValue(c.JobId, out var ordId)) continue;
            if (!costByOrder.TryGetValue(ordId, out var arr))
            {
                arr = new decimal[4];
                costByOrder[ordId] = arr;
            }
            var idx = c.Type switch
            {
                CostType.Material    => 0,
                CostType.Labor       => 1,
                CostType.Electricity => 2,
                CostType.ToolWear    => 3,
                _                    => -1,
            };
            if (idx >= 0) arr[idx] += c.Amount;
        }

        var jobCountByOrder = jobs.GroupBy(j => j.OrderId)
            .ToDictionary(g => g.Key, g => (Total: g.Count(), Done: g.Count(j => j.Status == JobStatus.Done)));

        var result = new List<OrderProfitLossDto>(orders.Count);
        foreach (var o in orders)
        {
            var revenue = o.Items.Sum(i => i.UnitPrice * i.Quantity);
            var arr = costByOrder.TryGetValue(o.Id, out var c) ? c : new decimal[4];
            var counts = jobCountByOrder.TryGetValue(o.Id, out var jc) ? jc : (Total: 0, Done: 0);

            result.Add(new OrderProfitLossDto(
                OrderId: o.Id,
                CustomerName: o.Customer?.Name ?? "—",
                OrderDate: o.OrderDate,
                Status: o.Status,
                Revenue: revenue,
                MaterialCost: arr[0],
                LaborCost: arr[1],
                ElectricityCost: arr[2],
                ToolWearCost: arr[3],
                TotalJobs: counts.Total,
                CompletedJobs: counts.Done));
        }

        return result;
    }

    private readonly record struct CostRow(int JobId, string Type, decimal Amount);
}
