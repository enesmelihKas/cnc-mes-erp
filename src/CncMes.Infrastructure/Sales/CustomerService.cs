using CncMes.Application.Sales;
using CncMes.Application.Sales.Requests;
using CncMes.Domain.Sales;
using CncMes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CncMes.Infrastructure.Sales;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CustomerDto>> ListAsync(string? search = null, CancellationToken ct = default)
    {
        var query = _db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{term}%") ||
                (c.Phone != null && EF.Functions.Like(c.Phone, $"%{term}%")) ||
                (c.Email != null && EF.Functions.Like(c.Email, $"%{term}%")));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                c.Phone,
                c.Email,
                c.TaxNumber,
                c.Notes,
                c.CreatedAt,
                c.Orders.Count))
            .ToListAsync(ct);
    }

    public async Task<CustomerDto?> GetAsync(int id, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(
                c.Id, c.Name, c.Phone, c.Email, c.TaxNumber, c.Notes, c.CreatedAt, c.Orders.Count))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Phone = NormalizeOrNull(request.Phone),
            Email = NormalizeOrNull(request.Email),
            TaxNumber = NormalizeOrNull(request.TaxNumber),
            Notes = NormalizeOrNull(request.Notes),
            CreatedAt = DateTime.UtcNow,
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        return (await GetAsync(customer.Id, ct))!;
    }

    public async Task<CustomerDto> UpdateAsync(int id, CustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı (Id={id}).");

        customer.Name = request.Name.Trim();
        customer.Phone = NormalizeOrNull(request.Phone);
        customer.Email = NormalizeOrNull(request.Email);
        customer.TaxNumber = NormalizeOrNull(request.TaxNumber);
        customer.Notes = NormalizeOrNull(request.Notes);

        await _db.SaveChangesAsync(ct);
        return (await GetAsync(id, ct))!;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var customer = await _db.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı (Id={id}).");

        if (customer.Orders.Any())
        {
            throw new InvalidOperationException(
                $"Bu müşteriye ait {customer.Orders.Count} sipariş var; önce siparişleri sil veya başka müşteriye taşı.");
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(ct);
    }

    private static string? NormalizeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}
