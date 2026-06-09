namespace CncMes.Application.Reporting;

/// <summary>
/// Bir siparişin kar/zarar özeti. Gelir = sipariş tutarı (Σ UnitPrice×Qty),
/// maliyet = o siparişin işlerinden üretilen CostEntry'lerin toplamı.
/// </summary>
public record OrderProfitLossDto(
    int OrderId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    decimal Revenue,
    decimal MaterialCost,
    decimal LaborCost,
    decimal ElectricityCost,
    decimal ToolWearCost,
    int TotalJobs,
    int CompletedJobs)
{
    public decimal TotalCost => MaterialCost + LaborCost + ElectricityCost + ToolWearCost;

    public decimal Profit => Revenue - TotalCost;

    /// <summary>Kar marjı (%). Gelir 0 ise 0 döner.</summary>
    public decimal MarginPercent =>
        Revenue > 0 ? Math.Round(Profit / Revenue * 100m, 1) : 0m;

    /// <summary>
    /// Tüm işler tamamlandı mı? Değilse maliyet henüz kısmi — kar/zarar
    /// "geçici" olarak yorumlanmalı (tamamlanmamış işlerin maliyeti yok).
    /// </summary>
    public bool IsFullyCosted => TotalJobs > 0 && CompletedJobs == TotalJobs;
}
