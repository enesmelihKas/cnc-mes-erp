namespace CncMes.Application.Sales;

public record OrderItemDto(
    int Id,
    int OrderId,
    string PartName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    bool IsRnD,
    string? StlFilePath
);
