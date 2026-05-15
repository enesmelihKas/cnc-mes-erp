using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Sales.Requests;

public class CustomerRequest
{
    [Required(ErrorMessage = "Müşteri adı zorunlu")]
    [StringLength(200, ErrorMessage = "Müşteri adı en fazla 200 karakter olabilir")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Telefon en fazla 50 karakter olabilir")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin")]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "Vergi numarası en fazla 50 karakter olabilir")]
    public string? TaxNumber { get; set; }

    [StringLength(2000, ErrorMessage = "Not en fazla 2000 karakter olabilir")]
    public string? Notes { get; set; }
}
