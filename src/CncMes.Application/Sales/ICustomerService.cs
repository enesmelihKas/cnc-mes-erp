using CncMes.Application.Sales.Requests;

namespace CncMes.Application.Sales;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(string? search = null, CancellationToken ct = default);
    Task<CustomerDto?> GetAsync(int id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerRequest request, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(int id, CustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
