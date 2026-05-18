using System.ComponentModel.DataAnnotations;

namespace CncMes.Application.Pricing;

public class PartFeaturesRequest
{
    [Required(ErrorMessage = "Hammadde seçimi zorunlu")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir hammadde seç")]
    public int MaterialId { get; set; }

    [Required(ErrorMessage = "Tolerans seçimi zorunlu")]
    [StringLength(30)]
    public string ToleranceGrade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yüzey işlem seçimi zorunlu")]
    [StringLength(30)]
    public string SurfaceFinish { get; set; } = string.Empty;

    [Range(0, 500, ErrorMessage = "Delik sayısı 0-500 aralığında olmalı")]
    public int HoleCount { get; set; }

    public bool HeatTreated { get; set; }

    [StringLength(500)]
    public string? ManualNotes { get; set; }
}
