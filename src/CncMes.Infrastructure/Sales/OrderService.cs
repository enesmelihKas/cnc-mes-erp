using CncMes.Application.Pricing;
using CncMes.Application.Production;
using CncMes.Application.Sales;
using CncMes.Application.Sales.Requests;
using CncMes.Domain.Pricing;
using CncMes.Domain.Sales;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Sales;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IStlAnalysisService _stl;

    public OrderService(AppDbContext db, IStlAnalysisService stl)
    {
        _db = db;
        _stl = stl;
    }

    public async Task<IReadOnlyList<OrderListItemDto>> ListAsync(string? statusFilter = null, CancellationToken ct = default)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.Id)
            .ToListAsync(ct);

        // Toplam fiyat ve kalem sayısı, SQLite'ın decimal aggregate sınırlamasından
        // dolayı C# tarafında hesaplanıyor (EF Core 8 + SQLite).
        return orders
            .Select(o => new OrderListItemDto(
                o.Id,
                o.CustomerId,
                o.Customer!.Name,
                o.OrderDate,
                o.DueDate,
                o.Status,
                o.Items.Sum(i => i.UnitPrice * i.Quantity),
                o.Items.Count))
            .ToList();
    }

    public async Task<OrderDetailDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null) return null;

        return new OrderDetailDto(
            order.Id,
            order.CustomerId,
            order.Customer!.Name,
            order.OrderDate,
            order.DueDate,
            order.Status,
            order.Notes,
            order.Items.Sum(i => i.UnitPrice * i.Quantity),
            order.CreatedAt,
            order.Items
                .OrderBy(i => i.Id)
                .Select(ToItemDto)
                .ToList());
    }

    public async Task<OrderItemDto?> GetItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems
            .AsNoTracking()
            .Include(i => i.Material)
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);
        return item is null ? null : ToItemDto(item);
    }

    private static OrderItemDto ToItemDto(OrderItem i) => new(
        Id: i.Id,
        OrderId: i.OrderId,
        PartName: i.PartName,
        Quantity: i.Quantity,
        UnitPrice: i.UnitPrice,
        LineTotal: i.UnitPrice * i.Quantity,
        IsRnD: i.IsRnD,
        StlFilePath: i.StlFilePath,
        StlFileName: i.StlFileName,
        VolumeCm3: i.VolumeCm3,
        SurfaceAreaCm2: i.SurfaceAreaCm2,
        BoundingBoxXCm: i.BoundingBoxXCm,
        BoundingBoxYCm: i.BoundingBoxYCm,
        BoundingBoxZCm: i.BoundingBoxZCm,
        TriangleCount: i.TriangleCount,
        AspectRatio: i.AspectRatio,
        Solidity: i.Solidity,
        ExcessMaterialRatio: i.ExcessMaterialRatio,
        IsClosedMesh: i.IsClosedMesh,
        MaterialId: i.MaterialId,
        MaterialName: i.Material?.Name,
        ToleranceGrade: i.ToleranceGrade,
        SurfaceFinish: i.SurfaceFinish,
        HoleCount: i.HoleCount,
        HeatTreated: i.HeatTreated,
        ManualNotes: i.ManualNotes,
        EstimatedMinutes: i.EstimatedMinutes,
        EstimatedUnitPrice: i.EstimatedUnitPrice,
        AnalyzedAt: i.AnalyzedAt);

    public async Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == request.CustomerId, ct);
        if (!customerExists)
        {
            throw new InvalidOperationException($"Geçerli bir müşteri seçilmedi (Id={request.CustomerId}).");
        }

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = request.OrderDate.Date,
            DueDate = request.DueDate?.Date,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return (await GetAsync(order.Id, ct))!;
    }

    public async Task<OrderDetailDto> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken ct = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı (Id={id}).");

        if (!OrderStatus.All.Contains(request.Status))
        {
            throw new InvalidOperationException($"Geçersiz sipariş durumu: '{request.Status}'.");
        }

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == request.CustomerId, ct);
        if (!customerExists)
        {
            throw new InvalidOperationException($"Geçerli bir müşteri seçilmedi (Id={request.CustomerId}).");
        }

        order.CustomerId = request.CustomerId;
        order.OrderDate = request.OrderDate.Date;
        order.DueDate = request.DueDate?.Date;
        order.Status = request.Status;
        order.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _db.SaveChangesAsync(ct);
        return (await GetAsync(id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı (Id={id}).");

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<OrderItemDto> AddItemAsync(int orderId, OrderItemRequest request, CancellationToken ct = default)
    {
        var orderExists = await _db.Orders.AnyAsync(o => o.Id == orderId, ct);
        if (!orderExists)
        {
            throw new KeyNotFoundException($"Sipariş bulunamadı (Id={orderId}).");
        }

        var item = new OrderItem
        {
            OrderId = orderId,
            PartName = request.PartName.Trim(),
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            IsRnD = request.IsRnD,
            ToleranceGrade = ToleranceGrade.Standard,
            SurfaceFinish = SurfaceFinish.AsMilled,
        };

        _db.OrderItems.Add(item);
        await _db.SaveChangesAsync(ct);

        return (await GetItemAsync(item.Id, ct))!;
    }

    public async Task RemoveItemAsync(int orderId, int itemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={itemId}).");

        _db.OrderItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<OrderItemDto> UpdatePartFeaturesAsync(
        int itemId,
        PartFeaturesRequest request,
        CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == itemId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={itemId}).");

        // MaterialId doğrulaması
        var material = await _db.Materials
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId, ct)
            ?? throw new InvalidOperationException("Seçilen hammadde bulunamadı.");
        if (!material.IsActive)
        {
            throw new InvalidOperationException($"Hammadde aktif değil: {material.Name}");
        }

        // ToleranceGrade / SurfaceFinish whitelist
        if (!ToleranceGrade.All.Contains(request.ToleranceGrade))
        {
            throw new InvalidOperationException($"Geçersiz tolerans: '{request.ToleranceGrade}'.");
        }
        if (!SurfaceFinish.All.Contains(request.SurfaceFinish))
        {
            throw new InvalidOperationException($"Geçersiz yüzey işlem: '{request.SurfaceFinish}'.");
        }

        item.MaterialId = request.MaterialId;
        item.ToleranceGrade = request.ToleranceGrade;
        item.SurfaceFinish = request.SurfaceFinish;
        item.HoleCount = request.HoleCount;
        item.HeatTreated = request.HeatTreated;
        item.ManualNotes = string.IsNullOrWhiteSpace(request.ManualNotes) ? null : request.ManualNotes.Trim();

        await _db.SaveChangesAsync(ct);

        // STL varsa fiyat tahminini yeniden hesapla
        if (!string.IsNullOrWhiteSpace(item.StlFilePath))
        {
            await _stl.RecalculateAsync(itemId, ct);
        }

        return (await GetItemAsync(itemId, ct))!;
    }
}
