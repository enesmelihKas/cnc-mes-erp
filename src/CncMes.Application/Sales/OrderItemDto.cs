namespace CncMes.Application.Sales;

public record OrderItemDto(
    int Id,
    int OrderId,
    string PartName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    bool IsRnD,
    string? StlFilePath,
    string? StlFileName = null,

    // STL geometri
    decimal? VolumeCm3 = null,
    decimal? SurfaceAreaCm2 = null,
    decimal? BoundingBoxXCm = null,
    decimal? BoundingBoxYCm = null,
    decimal? BoundingBoxZCm = null,
    int? TriangleCount = null,

    // Ek geometri metrikleri
    decimal? AspectRatio = null,
    decimal? Solidity = null,
    decimal? ExcessMaterialRatio = null,
    bool? IsClosedMesh = null,

    // Manager input
    int? MaterialId = null,
    string? MaterialName = null,
    string ToleranceGrade = "Standard",
    string SurfaceFinish = "AsMilled",
    int HoleCount = 0,
    bool HeatTreated = false,
    string? ManualNotes = null,

    // Tahminler
    decimal? EstimatedMinutes = null,
    decimal? EstimatedUnitPrice = null,
    DateTime? AnalyzedAt = null
)
{
    public bool HasStl => !string.IsNullOrWhiteSpace(StlFilePath);
}
