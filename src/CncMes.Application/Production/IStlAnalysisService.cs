namespace CncMes.Application.Production;

public interface IStlAnalysisService
{
    /// <summary>
    /// STL içeriğini analiz eder, OrderItem'a kaydeder, hesaplanan fiyatı döner.
    /// Mevcut STL varsa üzerine yazar (eski dosya silinir).
    /// Estimate, OrderItem'da kayıtlı features (Material, ToleranceGrade vs.) ile yapılır.
    /// </summary>
    Task<StlAnalysisResultDto> UploadAndAnalyzeAsync(
        int orderItemId,
        string originalFileName,
        Stream stlContent,
        CancellationToken ct = default);

    /// <summary>
    /// STL aynı, sadece features veya pricing değişti — tahmin yeniden hesaplanır,
    /// EstimatedMinutes/EstimatedUnitPrice kayıt edilir.
    /// </summary>
    Task RecalculateAsync(int orderItemId, CancellationToken ct = default);

    /// <summary>
    /// OrderItem'ın STL dosyasını ve analiz verilerini temizler (features korunur).
    /// </summary>
    Task RemoveAsync(int orderItemId, CancellationToken ct = default);

    /// <summary>
    /// Hesaplanmış EstimatedUnitPrice'ı OrderItem.UnitPrice'a kopyalar.
    /// </summary>
    Task ApplyEstimatedPriceAsync(int orderItemId, CancellationToken ct = default);
}
