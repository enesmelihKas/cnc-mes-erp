using CncMes.Application.Production;
using CncMes.Application.Production.Requests;
using CncMes.Domain.Identity;
using CncMes.Domain.Pricing;
using CncMes.Domain.Production;
using CncMes.Domain.Sales;
using CncMes.Infrastructure.Identity;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Production;

public class JobService : IJobService
{
    private readonly AppDbContext _db;

    public JobService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JobListItemDto>> ListAsync(
        string? statusFilter = null,
        int? operatorFilter = null,
        CancellationToken ct = default)
    {
        var rows = JoinRows();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            rows = rows.Where(x => x.J.Status == statusFilter);
        }

        if (operatorFilter is int opId)
        {
            rows = rows.Where(x => x.J.AssignedOperatorId == opId);
        }

        return await rows
            .OrderByDescending(x => x.J.CreatedAt)
            .Select(x => Project(x))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<JobListItemDto>> ListMineAsync(int operatorId, CancellationToken ct = default)
    {
        return await JoinRows()
            .Where(x => x.J.AssignedOperatorId == operatorId
                        && x.J.Status != JobStatus.Cancelled)
            .OrderBy(x => x.J.Status == JobStatus.Done ? 1 : 0)
            .ThenBy(x => x.O.DueDate)
            .ThenByDescending(x => x.J.CreatedAt)
            .Select(x => Project(x))
            .ToListAsync(ct);
    }

    public async Task<JobDetailDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var job = await _db.Jobs
            .AsNoTracking()
            .Include(j => j.OrderItem!).ThenInclude(i => i.Order!).ThenInclude(o => o.Customer)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (job is null) return null;

        var operatorName = job.AssignedOperatorId is int opId
            ? await GetOperatorNameAsync(opId, ct)
            : null;

        var item = job.OrderItem!;
        var order = item.Order!;

        return new JobDetailDto(
            job.Id,
            order.Id,
            item.Id,
            item.PartName,
            item.Quantity,
            item.IsRnD,
            order.CustomerId,
            order.Customer!.Name,
            job.Status,
            job.AssignedOperatorId,
            operatorName,
            job.StartedAt,
            job.FinishedAt,
            job.CreatedAt,
            order.DueDate,
            job.Notes);
    }

    public async Task<JobDetailDto> CreateForOrderItemAsync(int orderItemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems
            .Include(i => i.Jobs)
            .FirstOrDefaultAsync(i => i.Id == orderItemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={orderItemId}).");

        var activeExists = item.Jobs.Any(j => j.Status != JobStatus.Cancelled);
        if (activeExists)
        {
            throw new InvalidOperationException("Bu kalem için zaten aktif bir iş emri var.");
        }

        var job = new Job
        {
            OrderItemId = orderItemId,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync(ct);

        return (await GetAsync(job.Id, ct))!;
    }

    public async Task<int> CreateMissingForOrderAsync(int orderId, CancellationToken ct = default)
    {
        var items = await _db.OrderItems
            .Include(i => i.Jobs)
            .Where(i => i.OrderId == orderId)
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            throw new KeyNotFoundException($"Sipariş bulunamadı veya kalemi yok (Id={orderId}).");
        }

        var created = 0;
        foreach (var item in items)
        {
            if (item.Jobs.Any(j => j.Status != JobStatus.Cancelled)) continue;

            _db.Jobs.Add(new Job
            {
                OrderItemId = item.Id,
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            created++;
        }

        if (created > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return created;
    }

    public async Task<JobDetailDto> AssignAsync(int jobId, AssignJobRequest request, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.Status is JobStatus.Done or JobStatus.Cancelled)
        {
            throw new InvalidOperationException("Tamamlanmış veya iptal edilmiş iş emri atanamaz.");
        }

        var isOperator = await IsOperatorAsync(request.OperatorId, ct);
        if (!isOperator)
        {
            throw new InvalidOperationException("Seçilen kullanıcı operatör rolünde değil.");
        }

        job.AssignedOperatorId = request.OperatorId;
        if (job.Status == JobStatus.Pending)
        {
            job.Status = JobStatus.Assigned;
        }

        await _db.SaveChangesAsync(ct);
        return (await GetAsync(jobId, ct))!;
    }

    public async Task<JobDetailDto> UnassignAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.Status is JobStatus.InProgress)
        {
            throw new InvalidOperationException("Devam eden iş emrinin ataması kaldırılamaz; önce iptal et.");
        }
        if (job.Status is JobStatus.Done)
        {
            throw new InvalidOperationException("Tamamlanmış iş emrinin ataması değiştirilemez.");
        }

        job.AssignedOperatorId = null;
        if (job.Status == JobStatus.Assigned)
        {
            job.Status = JobStatus.Pending;
        }

        await _db.SaveChangesAsync(ct);
        return (await GetAsync(jobId, ct))!;
    }

    public async Task<JobDetailDto> StartAsync(int jobId, int actingOperatorId, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.AssignedOperatorId != actingOperatorId)
        {
            throw new InvalidOperationException("Bu iş emri sana atanmamış.");
        }
        if (job.Status != JobStatus.Assigned)
        {
            throw new InvalidOperationException($"İş emri başlatılamaz (mevcut durum: {JobStatus.ToTurkish(job.Status)}).");
        }

        job.Status = JobStatus.InProgress;
        job.StartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await TrySyncOrderStatusAsync(job.OrderItemId, ct);

        return (await GetAsync(jobId, ct))!;
    }

    public async Task<JobDetailDto> FinishAsync(int jobId, int actingOperatorId, JobNoteRequest request, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.AssignedOperatorId != actingOperatorId)
        {
            throw new InvalidOperationException("Bu iş emri sana atanmamış.");
        }
        if (job.Status != JobStatus.InProgress)
        {
            throw new InvalidOperationException($"İş emri bitirilemez (mevcut durum: {JobStatus.ToTurkish(job.Status)}).");
        }

        job.Status = JobStatus.Done;
        job.FinishedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            job.Notes = request.Notes.Trim();
        }

        await DeductMaterialStockAsync(job.OrderItemId, ct);
        await GenerateCostEntriesAsync(job, ct);

        await _db.SaveChangesAsync(ct);

        await TrySyncOrderStatusAsync(job.OrderItemId, ct);

        return (await GetAsync(jobId, ct))!;
    }

    /// <summary>
    /// İş tamamlanınca dört maliyet kalemini (Hammadde/İşçilik/Elektrik/Takım) üretir.
    /// Süre olarak gerçek geçen süre (FinishedAt-StartedAt) kullanılır; bu &lt;1 dk ise
    /// (örn. demo'da anında başlat→bitir) OrderItem.EstimatedMinutes'a düşülür ki rapor
    /// anlamlı kalsın. Çağıran SaveChanges yapar.
    /// </summary>
    private async Task GenerateCostEntriesAsync(Job job, CancellationToken ct)
    {
        var item = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Id == job.OrderItemId)
            .Select(i => new
            {
                i.MaterialId,
                i.VolumeCm3,
                i.Quantity,
                i.EstimatedMinutes,
            })
            .FirstOrDefaultAsync(ct);

        if (item is null) return;

        var settings = await _db.PricingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == PricingSettings.SingletonId, ct);
        if (settings is null) return;

        var now = DateTime.UtcNow;

        // --- İşleme süresi (dk) ---
        var actualMinutes = job.StartedAt is not null && job.FinishedAt is not null
            ? (decimal)(job.FinishedAt.Value - job.StartedAt.Value).TotalMinutes
            : 0m;
        var workMinutes = actualMinutes >= 1m ? actualMinutes : (item.EstimatedMinutes ?? 0m);
        var minutesSource = actualMinutes >= 1m ? "gerçek süre" : "tahmini süre";

        var qty = Math.Max(item.Quantity, 1);

        // --- 1) Hammadde ---
        if (item.MaterialId is int materialId && item.VolumeCm3 is decimal volume)
        {
            var material = await _db.Materials.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == materialId, ct);
            if (material is not null)
            {
                var amount = Math.Round(volume * material.CostPerCm3 * qty, 2);
                AddCost(CostType.Material, amount,
                    $"{volume:0.##} cm³ × {material.CostPerCm3:0.##} TL × {qty} adet");
            }
        }

        // --- 2) İşçilik ---
        var laborAmount = Math.Round(workMinutes * settings.LaborCostPerMinute, 2);
        AddCost(CostType.Labor, laborAmount,
            $"{workMinutes:0.##} dk ({minutesSource}) × {settings.LaborCostPerMinute:0.##} TL/dk");

        // --- 3) Elektrik ---
        var electricityAmount = Math.Round(
            (workMinutes / 60m) * settings.MachinePowerKw * settings.ElectricityCostPerKwh, 2);
        AddCost(CostType.Electricity, electricityAmount,
            $"{workMinutes / 60m:0.###} sa × {settings.MachinePowerKw:0.##} kW × {settings.ElectricityCostPerKwh:0.##} TL/kWh");

        // --- 4) Takım aşınması ---
        var toolAmount = Math.Round(workMinutes * settings.ToolWearCostPerMinute, 2);
        AddCost(CostType.ToolWear, toolAmount,
            $"{workMinutes:0.##} dk × {settings.ToolWearCostPerMinute:0.##} TL/dk");

        void AddCost(string type, decimal amount, string detail) =>
            _db.CostEntries.Add(new CostEntry
            {
                JobId = job.Id,
                Type = type,
                AmountTL = amount,
                Detail = detail,
                CreatedAt = now,
            });
    }

    /// <summary>
    /// İş tamamlanınca OrderItem'daki hammaddeden tüketilen kg'ı düşer.
    /// kg = Volume(cm³) × Density(g/cm³) × Quantity / 1000
    /// MaterialId veya VolumeCm3 yoksa sessizce atlanır.
    /// </summary>
    private async Task DeductMaterialStockAsync(int orderItemId, CancellationToken ct)
    {
        var item = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Id == orderItemId)
            .Select(i => new { i.MaterialId, i.VolumeCm3, i.Quantity })
            .FirstOrDefaultAsync(ct);

        if (item is null || item.MaterialId is not int materialId || item.VolumeCm3 is not decimal volume)
        {
            return;
        }

        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == materialId, ct);
        if (material is null) return;

        var consumedKg = volume * material.DensityGramPerCm3 * item.Quantity / 1000m;
        material.StockKg -= consumedKg;
    }

    public async Task<JobDetailDto> CancelAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.Status == JobStatus.Done)
        {
            throw new InvalidOperationException("Tamamlanmış iş emri iptal edilemez.");
        }

        job.Status = JobStatus.Cancelled;
        job.FinishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return (await GetAsync(jobId, ct))!;
    }

    public async Task DeleteAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı (Id={jobId}).");

        if (job.Status is JobStatus.InProgress)
        {
            throw new InvalidOperationException("Devam eden iş emri silinemez; önce iptal et.");
        }

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OperatorSummaryDto>> ListOperatorsAsync(CancellationToken ct = default)
    {
        return await (
            from u in _db.Users.AsNoTracking()
            join ur in _db.UserRoles on u.Id equals ur.UserId
            join r in _db.Roles on ur.RoleId equals r.Id
            where r.Name == Roles.Operator
            orderby u.UserName
            select new OperatorSummaryDto(u.Id, u.UserName!, u.FullName)
        ).ToListAsync(ct);
    }

    private sealed class JobJoinRow
    {
        public Job J { get; set; } = default!;
        public OrderItem I { get; set; } = default!;
        public Order O { get; set; } = default!;
        public Customer C { get; set; } = default!;
        public AppUser? U { get; set; }
    }

    private IQueryable<JobJoinRow> JoinRows() =>
        from j in _db.Jobs.AsNoTracking()
        join i in _db.OrderItems on j.OrderItemId equals i.Id
        join o in _db.Orders on i.OrderId equals o.Id
        join c in _db.Customers on o.CustomerId equals c.Id
        join u in _db.Users on j.AssignedOperatorId equals u.Id into uj
        from u in uj.DefaultIfEmpty()
        select new JobJoinRow { J = j, I = i, O = o, C = c, U = u };

    private static JobListItemDto Project(JobJoinRow x) => new(
        x.J.Id,
        x.O.Id,
        x.I.Id,
        x.I.PartName,
        x.I.Quantity,
        x.C.Id,
        x.C.Name,
        x.J.Status,
        x.J.AssignedOperatorId,
        x.U != null ? (x.U.FullName ?? x.U.UserName) : null,
        x.J.StartedAt,
        x.J.FinishedAt,
        x.J.CreatedAt,
        x.O.DueDate);

    private async Task<string?> GetOperatorNameAsync(int userId, CancellationToken ct)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FullName ?? u.UserName)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<bool> IsOperatorAsync(int userId, CancellationToken ct)
    {
        return await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && r.Name == Roles.Operator
            select ur.UserId
        ).AnyAsync(ct);
    }

    private async Task TrySyncOrderStatusAsync(int orderItemId, CancellationToken ct)
    {
        var orderId = await _db.OrderItems
            .Where(i => i.Id == orderItemId)
            .Select(i => i.OrderId)
            .FirstAsync(ct);

        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Jobs)
            .FirstAsync(o => o.Id == orderId, ct);

        var allItemsDone = order.Items.All(i =>
            i.Jobs.Any() && i.Jobs.All(j => j.Status is JobStatus.Done or JobStatus.Cancelled)
                         && i.Jobs.Any(j => j.Status == JobStatus.Done));

        if (allItemsDone && order.Status != OrderStatus.Completed)
        {
            order.Status = OrderStatus.Completed;
            await _db.SaveChangesAsync(ct);
        }
        else if (order.Items.Any(i => i.Jobs.Any(j => j.Status == JobStatus.InProgress))
                 && order.Status is OrderStatus.Draft or OrderStatus.Confirmed)
        {
            order.Status = OrderStatus.InProduction;
            await _db.SaveChangesAsync(ct);
        }
    }
}
