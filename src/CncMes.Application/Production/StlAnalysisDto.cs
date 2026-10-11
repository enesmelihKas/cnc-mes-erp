namespace CncMes.Application.Production;

public record StlGeometryDto(
    decimal VolumeCm3,
    decimal SurfaceAreaCm2,
    decimal BoundingBoxXCm,
    decimal BoundingBoxYCm,
    decimal BoundingBoxZCm,
    int TriangleCount,
    // --- Ek geometri metrikleri ---
    decimal AspectRatio,
    decimal Solidity,
    decimal ExcessMaterialRatio,
    bool IsClosedMesh
);

public record StlAnalysisResultDto(
    int OrderItemId,
    string StlFileName,
    string StlFilePath,
    StlGeometryDto Geometry,
    decimal EstimatedMinutes,
    decimal EstimatedUnitPrice,
    DateTime AnalyzedAt
);
