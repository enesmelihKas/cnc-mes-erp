namespace CncMes.Application.Sales;

public record OrderListItemDto(
    int Id,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    DateTime? DueDate,
    string Status,
    decimal TotalPrice,
    int ItemCount
);

public record OrderDetailDto(
    int Id,
    int CustomerId,
    string CustomerName,
    DateTime OrderDate,
    DateTime? DueDate,
    string Status,
    string? Notes,
    decimal TotalPrice,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items
);
