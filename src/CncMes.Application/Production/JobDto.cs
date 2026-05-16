namespace CncMes.Application.Production;

public record JobListItemDto(
    int Id,
    int OrderId,
    int OrderItemId,
    string PartName,
    int Quantity,
    int CustomerId,
    string CustomerName,
    string Status,
    int? AssignedOperatorId,
    string? AssignedOperatorName,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    DateTime CreatedAt,
    DateTime? DueDate
)
{
    public TimeSpan? Duration =>
        StartedAt is null
            ? null
            : (FinishedAt ?? DateTime.UtcNow) - StartedAt;
}

public record JobDetailDto(
    int Id,
    int OrderId,
    int OrderItemId,
    string PartName,
    int Quantity,
    bool IsRnD,
    int CustomerId,
    string CustomerName,
    string Status,
    int? AssignedOperatorId,
    string? AssignedOperatorName,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    DateTime CreatedAt,
    DateTime? DueDate,
    string? Notes
)
{
    public TimeSpan? Duration =>
        StartedAt is null
            ? null
            : (FinishedAt ?? DateTime.UtcNow) - StartedAt;
}

public record OperatorSummaryDto(
    int Id,
    string UserName,
    string? FullName
);
