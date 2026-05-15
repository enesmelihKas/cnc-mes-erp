using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Sales.Requests;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "Müşteri seçimi zorunlu")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir müşteri seç")]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Sipariş tarihi zorunlu")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    public DateTime? DueDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateOrderRequest
{
    [Required(ErrorMessage = "Müşteri seçimi zorunlu")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir müşteri seç")]
    public int CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Durum zorunlu")]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Notes { get; set; }
}
