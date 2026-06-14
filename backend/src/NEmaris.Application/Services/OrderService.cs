using Microsoft.Extensions.Options;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class TaxOptions
{
    public const string SectionName = "Tax";
    public decimal Rate { get; set; } = 0.20m;
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly decimal _taxRate;

    public OrderService(IOrderRepository repo, IOptions<TaxOptions> taxOptions)
    {
        _repo = repo;
        _taxRate = Math.Clamp(taxOptions.Value.Rate, 0m, 1m);
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
            TaxRate = _taxRate,
            OpenedAt = DateTime.UtcNow,
        };

        order = await _repo.AddOrderAsync(order);
        await _repo.UpdateTableStatusAsync(dto.TableId, TableStatus.Seated);

        return MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOrderAsync(long id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null) return null;
        ApplyTax(order);
        return MapToOrderDto(order);
    }

    public async Task<OrderDto?> GetOpenOrderByTableIdAsync(long tableId)
    {
        var order = await _repo.GetOpenOrderByTableIdAsync(tableId);
        if (order is null) return null;
        ApplyTax(order);
        return MapToOrderDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(string? status = null, bool todayOnly = true)
    {
        OrderStatus? parsed = null;
        if (status is not null && Enum.TryParse<OrderStatus>(status, true, out var s))
            parsed = s;

        DateTime? from = null, to = null;
        if (todayOnly)
        {
            var todayStart = DateTime.UtcNow.Date;
            from = todayStart;
            to = todayStart.AddDays(1);
        }

        var orders = await _repo.GetOrdersAsync(parsed, from, to);
        foreach (var o in orders) ApplyTax(o);
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

        var previousQuantity = item.Quantity;
        item.Quantity = dto.Quantity;
        item.LineTotal = item.UnitPrice * dto.Quantity;
        item.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateOrderItemAsync(item, previousQuantity);
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

        ApplyTax(order);
        return MapToBillDto(order);
    }

    public async Task<BillDto> ProcessPaymentAsync(long orderId, CreatePaymentDto dto)
    {
        var order = await _repo.GetBillAsync(orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} not found.");

        if (order.Status != OrderStatus.Open)
            throw new InvalidOperationException("Cannot process payment for a non-open order.");

        ApplyTax(order);

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
        ApplyTax(bill!);
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

    public async Task<DailyStatsDto> GetTodayStatsAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrow = todayStart.AddDays(1);

        var orders = await _repo.GetOrdersForStatsAsync(todayStart, tomorrow);
        foreach (var o in orders) ApplyTax(o);

        var subtotal = orders.Sum(o => o.Subtotal - o.DiscountAmount);
        var tax = orders.Sum(o => o.TaxAmount);
        var revenue = orders.Sum(o => o.TotalAmount);

        var byPaymentMethod = orders
            .SelectMany(o => o.Payments)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodTotalDto
            {
                PaymentMethod = g.Key.ToString().ToLower(),
                Amount = g.Sum(p => p.Amount),
                Count = g.Count(),
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var topItems = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.MenuItemId, Name = i.MenuItem?.Name ?? string.Empty })
            .Select(g => new TopItemDto
            {
                MenuItemId = g.Key.MenuItemId,
                MenuItemName = g.Key.Name,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal),
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();

        var byWaiter = orders
            .GroupBy(o => new { o.WaiterUserId, Name = o.Waiter?.FullName ?? string.Empty })
            .Select(g => new WaiterTotalDto
            {
                WaiterUserId = g.Key.WaiterUserId,
                WaiterName = g.Key.Name,
                BillCount = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount),
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        return new DailyStatsDto
        {
            Date = DateTime.SpecifyKind(todayStart, DateTimeKind.Utc),
            BillCount = orders.Count,
            Revenue = revenue,
            TaxCollected = tax,
            Subtotal = subtotal,
            ByPaymentMethod = byPaymentMethod,
            TopItems = topItems,
            ByWaiter = byWaiter,
        };
    }

    private void ApplyTax(Order o)
    {
        var rate = o.TaxRate > 0 ? o.TaxRate : _taxRate;
        var afterDiscount = Math.Max(o.Subtotal - o.DiscountAmount, 0m);
        o.TaxRate = rate;
        o.TaxAmount = Math.Round(afterDiscount * rate, 2, MidpointRounding.AwayFromZero);
        o.TotalAmount = afterDiscount + o.TaxAmount;
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
        TaxRate = o.TaxRate,
        TaxAmount = o.TaxAmount,
        TotalAmount = o.TotalAmount,
        OpenedAt = AsUtc(o.OpenedAt),
        ClosedAt = AsUtcNullable(o.ClosedAt),
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
        TaxRate = o.TaxRate,
        TaxAmount = o.TaxAmount,
        TotalAmount = o.TotalAmount,
        Payments = o.Payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            PaymentMethod = p.PaymentMethod.ToString().ToLower(),
            Amount = p.Amount,
            ReferenceNumber = p.ReferenceNumber,
            PaidAt = AsUtc(p.PaidAt),
        }).ToList(),
        OpenedAt = AsUtc(o.OpenedAt),
        ClosedAt = AsUtcNullable(o.ClosedAt),
    };

    private static DateTime AsUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static DateTime? AsUtcNullable(DateTime? value) =>
        value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}
