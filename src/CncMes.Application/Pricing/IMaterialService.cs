using CncMes.Application.Pricing.Requests;

namespace CncMes.Application.Pricing;

public interface IMaterialService
{
    /// <summary>Aktif (varsayılan) ya da tüm hammaddeleri listele.</summary>
    Task<IReadOnlyList<MaterialDto>> ListAsync(bool includeInactive = false, CancellationToken ct = default);

    Task<MaterialDto?> GetAsync(int id, CancellationToken ct = default);

    Task<MaterialDto> CreateAsync(MaterialRequest request, CancellationToken ct = default);

    Task<MaterialDto> UpdateAsync(int id, MaterialRequest request, CancellationToken ct = default);

    /// <summary>Stoğa kg ekle/düş (manuel ayarlama). Negatif delta stoğu düşürür.</summary>
    Task<MaterialDto> AdjustStockAsync(int id, StockAdjustmentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Hammaddeyi sil. Eğer mevcut OrderItem'lar bu hammaddeyi kullanıyorsa
    /// fiziksel silme yerine IsActive=false yapar.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Eşiğin altına düşmüş hammaddeler (dashboard uyarısı için).</summary>
    Task<IReadOnlyList<MaterialDto>> ListLowStockAsync(CancellationToken ct = default);
}
