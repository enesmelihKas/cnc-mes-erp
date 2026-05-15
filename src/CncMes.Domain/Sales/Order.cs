namespace CncMes.Domain.Sales;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; }
    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = OrderStatus.Draft;
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public decimal TotalPrice => Items.Sum(i => i.LineTotal);
}
