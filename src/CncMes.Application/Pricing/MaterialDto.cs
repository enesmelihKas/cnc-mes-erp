namespace CncMes.Application.Pricing;

public record MaterialDto(
    int Id,
    string Name,
    decimal CostPerCm3,
    decimal MachinabilityFactor,
    decimal DensityGramPerCm3,
    decimal StockKg,
    decimal LowStockThreshold,
    bool IsActive
)
{
    public bool IsBelowThreshold => StockKg < LowStockThreshold;
}
