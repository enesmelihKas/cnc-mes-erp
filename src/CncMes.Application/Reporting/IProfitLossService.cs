namespace CncMes.Application.Reporting;

/// <summary>
/// Kar/zarar raporlaması. Maliyet kalemleri (CostEntry) işler tamamlandıkça
/// üretilir; bu servis onları sipariş ve ay bazında toplar.
/// </summary>
public interface IProfitLossService
{
    /// <summary>Tüm siparişlerin kar/zarar özeti (en yeni sipariş üstte).</summary>
    Task<IReadOnlyList<OrderProfitLossDto>> ListOrdersAsync(CancellationToken ct = default);

    /// <summary>Tek bir siparişin kar/zarar özeti.</summary>
    Task<OrderProfitLossDto?> GetOrderAsync(int orderId, CancellationToken ct = default);

    /// <summary>Bir siparişin tüm maliyet satırları (parça + tür kırılımı).</summary>
    Task<IReadOnlyList<OrderCostLineDto>> ListOrderCostLinesAsync(int orderId, CancellationToken ct = default);

    /// <summary>Aylık kar/zarar özeti (en yeni ay üstte).</summary>
    Task<IReadOnlyList<MonthlyProfitLossDto>> GetMonthlySummaryAsync(CancellationToken ct = default);
}
