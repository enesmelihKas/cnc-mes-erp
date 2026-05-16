using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Production.Requests;

public class AssignJobRequest
{
    [Required(ErrorMessage = "Operatör seçimi zorunlu")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir operatör seç")]
    public int OperatorId { get; set; }
}

public class JobNoteRequest
{
    [StringLength(2000)]
    public string? Notes { get; set; }
}
