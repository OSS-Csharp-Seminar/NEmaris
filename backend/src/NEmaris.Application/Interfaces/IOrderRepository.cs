using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id);
    Task<Order?> GetBillAsync(long id);
    Task<Order?> GetOpenOrderByTableIdAsync(long tableId);
    Task<RestaurantTables?> GetTableByIdAsync(long tableId);
    Task<IReadOnlyList<Order>> GetOrdersAsync(OrderStatus? status = null);
    Task<Order> AddOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task<OrderItem> AddOrderItemAsync(OrderItem item);
    Task<OrderItem?> GetOrderItemByIdAsync(long itemId);
    Task UpdateOrderItemAsync(OrderItem item);
    Task RemoveOrderItemAsync(OrderItem item);
    Task<Payment> AddPaymentAsync(Payment payment);
    Task UpdateTableStatusAsync(long tableId, TableStatus status);
}
