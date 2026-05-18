using CncMes.Application.Production;
using IxMilia.Stl;

namespace CncMes.Infrastructure.Production;

/// <summary>
/// IxMilia.Stl ile parse edilmiş bir STL dosyasından geometri çıkarır.
/// Varsayım: STL içerikleri milimetre (mm) biriminde — CAD'in standardı.
/// Çıktı her zaman santimetre cinsindendir.
/// </summary>
internal static class StlGeometryCalculator
{
    public static StlGeometryDto Calculate(StlFile stlFile)
    {
        if (stlFile.Triangles.Count == 0)
        {
            throw new InvalidOperationException("STL dosyasında üçgen yok (boş veya bozuk).");
        }

        double volumeMm3 = 0d;
        double surfaceMm2 = 0d;
        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
        int triangleCount = 0;

        // Watertight kontrolü için edge → kullanım sayısı haritası.
        // Vertex koordinatları 4 ondalık basamağa yuvarlanarak string-key edge üretiyoruz
        // (STL'de aynı köşe noktası iki üçgenin tepesi olduğunda floating-point
        // farkları olabiliyor — yuvarlama gerekli).
        var edgeCounts = new Dictionary<string, int>(stlFile.Triangles.Count * 3);

        foreach (var t in stlFile.Triangles)
        {
            var v1 = (X: (double)t.Vertex1.X, Y: (double)t.Vertex1.Y, Z: (double)t.Vertex1.Z);
            var v2 = (X: (double)t.Vertex2.X, Y: (double)t.Vertex2.Y, Z: (double)t.Vertex2.Z);
            var v3 = (X: (double)t.Vertex3.X, Y: (double)t.Vertex3.Y, Z: (double)t.Vertex3.Z);

            volumeMm3 += SignedTetraVolume(v1, v2, v3);
            surfaceMm2 += TriangleArea(v1, v2, v3);

            UpdateBounds(v1, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
            UpdateBounds(v2, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);
            UpdateBounds(v3, ref minX, ref minY, ref minZ, ref maxX, ref maxY, ref maxZ);

            RecordEdge(edgeCounts, v1, v2);
            RecordEdge(edgeCounts, v2, v3);
            RecordEdge(edgeCounts, v3, v1);

            triangleCount++;
        }

        // mm → cm dönüşümü: linear /10, area /100, volume /1000.
        var volumeCm3 = Math.Abs(volumeMm3) / 1000d;
        var surfaceCm2 = surfaceMm2 / 100d;
        var bbX = (maxX - minX) / 10d;
        var bbY = (maxY - minY) / 10d;
        var bbZ = (maxZ - minZ) / 10d;

        // --- Phase 4.1 zenginleştirme ---
        var dims = new[] { bbX, bbY, bbZ };
        var maxDim = dims.Max();
        var minDim = Math.Max(dims.Min(), 0.001d); // sıfıra bölme koruması
        var aspectRatio = maxDim / minDim;

        var bboxVolume = bbX * bbY * bbZ;
        var solidity = bboxVolume > 0 ? volumeCm3 / bboxVolume : 0d;
        solidity = Math.Clamp(solidity, 0d, 1d);

        var excessRatio = volumeCm3 > 0
            ? bboxVolume / volumeCm3
            : 999d;

        // Watertight: her edge tam 2 üçgene ait mi?
        var isClosedMesh = edgeCounts.Values.All(c => c == 2);

        return new StlGeometryDto(
            VolumeCm3:           (decimal)Math.Round(volumeCm3, 4),
            SurfaceAreaCm2:      (decimal)Math.Round(surfaceCm2, 4),
            BoundingBoxXCm:      (decimal)Math.Round(bbX, 4),
            BoundingBoxYCm:      (decimal)Math.Round(bbY, 4),
            BoundingBoxZCm:      (decimal)Math.Round(bbZ, 4),
            TriangleCount:       triangleCount,
            AspectRatio:         (decimal)Math.Round(aspectRatio, 4),
            Solidity:            (decimal)Math.Round(solidity, 4),
            ExcessMaterialRatio: (decimal)Math.Round(excessRatio, 4),
            IsClosedMesh:        isClosedMesh);
    }

    private static double SignedTetraVolume(
        (double X, double Y, double Z) a,
        (double X, double Y, double Z) b,
        (double X, double Y, double Z) c)
    {
        var cx = b.Y * c.Z - b.Z * c.Y;
        var cy = b.Z * c.X - b.X * c.Z;
        var cz = b.X * c.Y - b.Y * c.X;
        return (a.X * cx + a.Y * cy + a.Z * cz) / 6d;
    }

    private static double TriangleArea(
        (double X, double Y, double Z) v1,
        (double X, double Y, double Z) v2,
        (double X, double Y, double Z) v3)
    {
        var ax = v2.X - v1.X; var ay = v2.Y - v1.Y; var az = v2.Z - v1.Z;
        var bx = v3.X - v1.X; var by = v3.Y - v1.Y; var bz = v3.Z - v1.Z;

        var cx = ay * bz - az * by;
        var cy = az * bx - ax * bz;
        var cz = ax * by - ay * bx;

        return Math.Sqrt(cx * cx + cy * cy + cz * cz) / 2d;
    }

    private static void UpdateBounds(
        (double X, double Y, double Z) v,
        ref double minX, ref double minY, ref double minZ,
        ref double maxX, ref double maxY, ref double maxZ)
    {
        if (v.X < minX) minX = v.X;
        if (v.Y < minY) minY = v.Y;
        if (v.Z < minZ) minZ = v.Z;
        if (v.X > maxX) maxX = v.X;
        if (v.Y > maxY) maxY = v.Y;
        if (v.Z > maxZ) maxZ = v.Z;
    }

    private static void RecordEdge(
        Dictionary<string, int> edges,
        (double X, double Y, double Z) a,
        (double X, double Y, double Z) b)
    {
        // Yönsüz edge: küçük olan vertex önce, böylece (a,b) ve (b,a) aynı key.
        var ka = VertexKey(a);
        var kb = VertexKey(b);
        var key = string.CompareOrdinal(ka, kb) < 0 ? ka + "|" + kb : kb + "|" + ka;
        edges[key] = edges.TryGetValue(key, out var c) ? c + 1 : 1;
    }

    private static string VertexKey((double X, double Y, double Z) v)
        => $"{v.X:F4}_{v.Y:F4}_{v.Z:F4}";
}
