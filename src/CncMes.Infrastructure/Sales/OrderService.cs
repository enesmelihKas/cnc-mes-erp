using CncMes.Application.Sales;
using CncMes.Application.Sales.Requests;
using CncMes.Domain.Sales;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Sales;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
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
            .Include(o => o.Items)
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
                .Select(i => new OrderItemDto(
                    i.Id, i.OrderId, i.PartName, i.Quantity,
                    i.UnitPrice, i.UnitPrice * i.Quantity,
                    i.IsRnD, i.StlFilePath))
                .ToList());
    }

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
        };

        _db.OrderItems.Add(item);
        await _db.SaveChangesAsync(ct);

        return new OrderItemDto(
            item.Id, item.OrderId, item.PartName, item.Quantity,
            item.UnitPrice, item.UnitPrice * item.Quantity,
            item.IsRnD, item.StlFilePath);
    }

    public async Task RemoveItemAsync(int orderId, int itemId, CancellationToken ct = default)
    {
        var item = await _db.OrderItems.FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId, ct)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı (Id={itemId}).");

        _db.OrderItems.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}
