namespace CncMes.Domain.Sales;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public string? StlFilePath { get; set; }
    public bool IsRnD { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
