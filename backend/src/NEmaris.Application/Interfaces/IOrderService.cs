using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string waiterUserId);
    Task<OrderDto?> GetOrderAsync(long id);
    Task<OrderDto?> GetOpenOrderByTableIdAsync(long tableId);
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? status = null);
    Task<OrderItemDto> AddOrderItemAsync(long orderId, AddOrderItemDto dto);
    Task<OrderItemDto> UpdateOrderItemAsync(long orderId, long itemId, UpdateOrderItemDto dto);
    Task RemoveOrderItemAsync(long orderId, long itemId);
    Task<BillDto> GetBillAsync(long orderId);
    Task<BillDto> ProcessPaymentAsync(long orderId, CreatePaymentDto dto);
    Task<OrderDto> CancelOrderAsync(long id);
}
