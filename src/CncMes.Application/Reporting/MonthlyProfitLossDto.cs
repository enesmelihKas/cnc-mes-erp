namespace CncMes.Application.Reporting;

/// <summary>
/// Aylık kar/zarar özeti. Siparişler <c>OrderDate</c> ayına göre gruplanır;
/// o ayın siparişlerinin geliri ve (işlerinden doğan) maliyeti toplanır.
/// </summary>
public record MonthlyProfitLossDto(
    int Year,
    int Month,
    decimal Revenue,
    decimal TotalCost,
    int OrderCount)
{
    public decimal Profit => Revenue - TotalCost;

    public decimal MarginPercent =>
        Revenue > 0 ? Math.Round(Profit / Revenue * 100m, 1) : 0m;

    /// <summary>"2026-06" gibi sıralanabilir etiket.</summary>
    public string Label => $"{Year:D4}-{Month:D2}";
}
