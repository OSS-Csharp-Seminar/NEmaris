using NEmaris.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace NEmaris.Application.DTOs;

public class CreateOrderDto
{
    [Required]
    [Range(1, long.MaxValue)]
    public long TableId { get; set; }

    public long? GuestId { get; set; }
    public long? ReservationId { get; set; }
}

public class OrderDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long TableId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public string WaiterUserId { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
    public long? GuestId { get; set; }
    public long? ReservationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class AddOrderItemDto
{
    [Required]
    [Range(1, long.MaxValue)]
    public long MenuItemId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class UpdateOrderItemDto
{
    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class OrderItemDto
{
    public long Id { get; set; }
    public long MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class BillDto
{
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PaymentDto> Payments { get; set; } = [];
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class DailyStatsDto
{
    public DateTime Date { get; set; }
    public int BillCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal TaxCollected { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tips { get; set; }
    public List<PaymentMethodTotalDto> ByPaymentMethod { get; set; } = [];
    public List<TopItemDto> TopItems { get; set; } = [];
    public List<WaiterTotalDto> ByWaiter { get; set; } = [];
}

public class PaymentMethodTotalDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class TopItemDto
{
    public long MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class WaiterTotalDto
{
    public string WaiterUserId { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
    public int BillCount { get; set; }
    public decimal Revenue { get; set; }
}

public class CreatePaymentDto
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Required]
    [Range(0.01, 999999.99)]
    public decimal Amount { get; set; }
}

public class PaymentDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
}
