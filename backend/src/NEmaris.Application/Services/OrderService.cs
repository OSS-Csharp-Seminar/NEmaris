using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string waiterUserId)
    {
        var existing = await _repo.GetOpenOrderByTableIdAsync(dto.TableId);
        if (existing != null)
            throw new InvalidOperationException($"Table already has an open order ({existing.OrderNumber}).");

        var table = await _repo.GetTableByIdAsync(dto.TableId)
            ?? throw new KeyNotFoundException($"Table {dto.TableId} not found.");

        if (table.Status != TableStatus.Seated)
            throw new InvalidOperationException("An order can only be opened for an occupied table.");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            TableId = dto.TableId,
            WaiterUserId = waiterUserId,
            GuestId = dto.GuestId,
            ReservationId = dto.ReservationId,
            Status = OrderStatus.Open,
            PaymentStatus = PaymentStatus.Unpaid,
            OpenedAt = DateTime.UtcNow,
        };

        order = await _repo.AddOrderAsync(order);
        await _repo.UpdateTableStatusAsync(dto.TableId, TableStatus.Seated);

        return MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOrderAsync(long id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order is null ? null : MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOpenOrderByTableIdAsync(long tableId)
    {
        var order = await _repo.GetOpenOrderByTableIdAsync(tableId);
        return order is null ? null : MapToOrderDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? status = null)
    {
        OrderStatus? parsed = null;
        if (status is not null && Enum.TryParse<OrderStatus>(status, true, out var s))
            parsed = s;

        var orders = await _repo.GetOrdersAsync(parsed);
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<OrderItemDto> AddOrderItemAsync(long orderId, AddOrderItemDto dto)
    {
        var order = await _repo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Cannot add items to a non-open order.");

        var item = new OrderItem
        {
            OrderId = orderId,
            MenuItemId = dto.MenuItemId,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        item = await _repo.AddOrderItemAsync(item);
        return MapToOrderItemDto(item);
    }

    public async Task<OrderItemDto> UpdateOrderItemAsync(long orderId, long itemId, UpdateOrderItemDto dto)
    {
        var order = await _repo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Cannot modify items on a non-open order.");

        var item = await _repo.GetOrderItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException($"Order item {itemId} not found.");

        if (item.OrderId != orderId)
            throw new InvalidOperationException("Item does not belong to this order.");

        item.Quantity = dto.Quantity;
        item.LineTotal = item.UnitPrice * dto.Quantity;
        item.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateOrderItemAsync(item);
        return MapToOrderItemDto(item);
    }

    public async Task RemoveOrderItemAsync(long orderId, long itemId)
    {
        var order = await _repo.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Cannot remove items from a non-open order.");

        var item = await _repo.GetOrderItemByIdAsync(itemId)
            ?? throw new KeyNotFoundException($"Order item {itemId} not found.");

        if (item.OrderId != orderId)
            throw new InvalidOperationException("Item does not belong to this order.");

        await _repo.RemoveOrderItemAsync(item);
    }

    public async Task<BillDto> GetBillAsync(long orderId)
    {
        var order = await _repo.GetBillAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        return MapToBillDto(order);
    }

    public async Task<BillDto> ProcessPaymentAsync(long orderId, CreatePaymentDto dto)
    {
        var order = await _repo.GetBillAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Cannot process payment for a non-open order.");

        if (dto.Amount < order.TotalAmount)
            throw new InvalidOperationException(
                $"Payment amount ({dto.Amount:F2}) is less than the order total ({order.TotalAmount:F2}).");

        var payment = new Payment
        {
            OrderId = orderId,
            PaymentMethod = dto.PaymentMethod,
            Amount = dto.Amount,
            ReferenceNumber = GenerateReferenceNumber(),
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await _repo.AddPaymentAsync(payment);

        order.Status = OrderStatus.Closed;
        order.PaymentStatus = PaymentStatus.Paid;
        order.ClosedAt = DateTime.UtcNow;
        await _repo.UpdateOrderAsync(order);
        await _repo.UpdateTableStatusAsync(order.TableId, TableStatus.Available);

        var bill = await _repo.GetBillAsync(orderId);
        return MapToBillDto(bill!);
    }

    public async Task<OrderDto> CancelOrderAsync(long id)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Order {id} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Only open orders can be cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.ClosedAt = DateTime.UtcNow;
        await _repo.UpdateOrderAsync(order);
        await _repo.UpdateTableStatusAsync(order.TableId, TableStatus.Available);

        return MapToOrderDto(order);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    private static string GenerateReferenceNumber()
        => $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

    private static OrderDto MapToOrderDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        TableId = o.TableId,
        TableNumber = o.Table?.TableNumber ?? string.Empty,
        WaiterUserId = o.WaiterUserId,
        WaiterName = o.Waiter?.FullName ?? string.Empty,
        GuestId = o.GuestId,
        ReservationId = o.ReservationId,
        Status = o.Status.ToString().ToLower(),
        PaymentStatus = o.PaymentStatus.ToString().ToLower(),
        Subtotal = o.Subtotal,
        DiscountAmount = o.DiscountAmount,
        TotalAmount = o.TotalAmount,
        OpenedAt = o.OpenedAt,
        ClosedAt = o.ClosedAt,
        Items = o.Items.Select(MapToOrderItemDto).ToList(),
    };

    private static OrderItemDto MapToOrderItemDto(OrderItem i) => new()
    {
        Id = i.Id,
        MenuItemId = i.MenuItemId,
        MenuItemName = i.MenuItem?.Name ?? string.Empty,
        Quantity = i.Quantity,
        UnitPrice = i.UnitPrice,
        LineTotal = i.LineTotal,
    };

    private static BillDto MapToBillDto(Order o) => new()
    {
        OrderId = o.Id,
        OrderNumber = o.OrderNumber,
        TableNumber = o.Table?.TableNumber ?? string.Empty,
        WaiterName = o.Waiter?.FullName ?? string.Empty,
        Status = o.Status.ToString().ToLower(),
        PaymentStatus = o.PaymentStatus.ToString().ToLower(),
        Items = o.Items.Select(MapToOrderItemDto).ToList(),
        Subtotal = o.Subtotal,
        DiscountAmount = o.DiscountAmount,
        TotalAmount = o.TotalAmount,
        Payments = o.Payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            PaymentMethod = p.PaymentMethod.ToString().ToLower(),
            Amount = p.Amount,
            ReferenceNumber = p.ReferenceNumber,
            PaidAt = p.PaidAt,
        }).ToList(),
        OpenedAt = o.OpenedAt,
        ClosedAt = o.ClosedAt,
    };
}
