using CncMes.Application.Sales.Requests;

namespace CncMes.Application.Sales;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListItemDto>> ListAsync(string? statusFilter = null, CancellationToken ct = default);
    Task<OrderDetailDto?> GetAsync(int id, CancellationToken ct = default);

    Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDetailDto> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<OrderItemDto?> GetItemAsync(int itemId, CancellationToken ct = default);
    Task<OrderItemDto> AddItemAsync(int orderId, OrderItemRequest request, CancellationToken ct = default);
    Task RemoveItemAsync(int orderId, int itemId, CancellationToken ct = default);

    /// <summary>
    /// Manager parça özelliklerini günceller (hammadde, tolerans, yüzey, vs.).
    /// Kayıt sonrası tahmin yeniden hesaplanır (STL varsa).
    /// </summary>
    Task<OrderItemDto> UpdatePartFeaturesAsync(
        int itemId,
        CncMes.Application.Pricing.PartFeaturesRequest request,
        CancellationToken ct = default);
}
