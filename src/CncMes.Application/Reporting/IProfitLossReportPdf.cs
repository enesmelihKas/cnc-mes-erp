namespace CncMes.Application.Reporting;

/// <summary>
/// Sipariş bazlı kar/zarar raporunu PDF baytlarına dönüştürür.
/// Implementasyon Infrastructure'da (QuestPDF) — Application yalnızca sözleşmeyi bilir.
/// </summary>
public interface IProfitLossReportPdf
{
    /// <summary>Bir siparişin kar/zarar raporunu PDF olarak üretir.</summary>
    byte[] BuildOrderReport(OrderProfitLossDto summary, IReadOnlyList<OrderCostLineDto> lines);
}
