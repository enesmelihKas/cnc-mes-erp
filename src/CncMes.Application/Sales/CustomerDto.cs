namespace CncMes.Application.Sales;

public record CustomerDto(
    int Id,
    string Name,
    string? Phone,
    string? Email,
    string? TaxNumber,
    string? Notes,
    DateTime CreatedAt,
    int OrderCount
);
