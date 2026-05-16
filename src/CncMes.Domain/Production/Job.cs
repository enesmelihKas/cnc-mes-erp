using CncMes.Domain.Sales;

namespace CncMes.Domain.Production;

public class Job
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public int? AssignedOperatorId { get; set; }

    public string Status { get; set; } = JobStatus.Pending;

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Notes { get; set; }

    public TimeSpan? Duration =>
        StartedAt is null
            ? null
            : (FinishedAt ?? DateTime.UtcNow) - StartedAt;
}
