namespace CncMes.Application.Reporting;

/// <summary>
/// Sipariş detayında ve PDF raporunda gösterilen tek bir maliyet satırı:
/// hangi iş/parça, hangi maliyet türü, ne kadar.
/// </summary>
public record OrderCostLineDto(
    int JobId,
    string PartName,
    string Type,
    string TypeLabel,
    decimal AmountTL,
    string? Detail);
