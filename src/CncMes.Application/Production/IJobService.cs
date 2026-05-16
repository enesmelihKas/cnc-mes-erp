using CncMes.Application.Production.Requests;

namespace CncMes.Application.Production;

public interface IJobService
{
    Task<IReadOnlyList<JobListItemDto>> ListAsync(
        string? statusFilter = null,
        int? operatorFilter = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<JobListItemDto>> ListMineAsync(
        int operatorId,
        CancellationToken ct = default);

    Task<JobDetailDto?> GetAsync(int id, CancellationToken ct = default);

    Task<JobDetailDto> CreateForOrderItemAsync(int orderItemId, CancellationToken ct = default);
    Task<int> CreateMissingForOrderAsync(int orderId, CancellationToken ct = default);

    Task<JobDetailDto> AssignAsync(int jobId, AssignJobRequest request, CancellationToken ct = default);
    Task<JobDetailDto> UnassignAsync(int jobId, CancellationToken ct = default);

    Task<JobDetailDto> StartAsync(int jobId, int actingOperatorId, CancellationToken ct = default);
    Task<JobDetailDto> FinishAsync(int jobId, int actingOperatorId, JobNoteRequest request, CancellationToken ct = default);

    Task<JobDetailDto> CancelAsync(int jobId, CancellationToken ct = default);
    Task DeleteAsync(int jobId, CancellationToken ct = default);

    Task<IReadOnlyList<OperatorSummaryDto>> ListOperatorsAsync(CancellationToken ct = default);
}
