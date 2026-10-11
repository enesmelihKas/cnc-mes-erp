using CncMes.Domain.Pricing;
using CncMes.Domain.Production;

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
    public string? StlFileName { get; set; }
    public bool IsRnD { get; set; }

    // --- Manager input ---
    public int? MaterialId { get; set; }
    public Material? Material { get; set; }

    public string ToleranceGrade { get; set; } = CncMes.Domain.Pricing.ToleranceGrade.Standard;
    public string SurfaceFinish { get; set; } = CncMes.Domain.Pricing.SurfaceFinish.AsMilled;
    public int HoleCount { get; set; }
    public bool HeatTreated { get; set; }
    public string? ManualNotes { get; set; }

    // --- STL analiz sonuçları (geometri) ---
    public decimal? VolumeCm3 { get; set; }
    public decimal? SurfaceAreaCm2 { get; set; }
    public decimal? BoundingBoxXCm { get; set; }
    public decimal? BoundingBoxYCm { get; set; }
    public decimal? BoundingBoxZCm { get; set; }
    public int? TriangleCount { get; set; }

    // --- Ek geometri metrikleri ---
    public decimal? AspectRatio { get; set; }
    public decimal? Solidity { get; set; }
    public decimal? ExcessMaterialRatio { get; set; }
    public bool? IsClosedMesh { get; set; }

    // --- Tahminler ---
    public decimal? EstimatedMinutes { get; set; }
    public decimal? EstimatedUnitPrice { get; set; }
    public DateTime? AnalyzedAt { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public decimal LineTotal => UnitPrice * Quantity;
}
