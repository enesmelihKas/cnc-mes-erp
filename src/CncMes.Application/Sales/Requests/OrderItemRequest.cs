using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Sales.Requests;

public class OrderItemRequest
{
    [Required(ErrorMessage = "Parça adı zorunlu")]
    [StringLength(200, ErrorMessage = "Parça adı en fazla 200 karakter olabilir")]
    public string PartName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Adet en az 1 olmalı")]
    public int Quantity { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz")]
    public decimal UnitPrice { get; set; }

    public bool IsRnD { get; set; }
}
