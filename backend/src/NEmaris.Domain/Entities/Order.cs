using NEmaris.Domain.Enums;

namespace NEmaris.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long TableId { get; set; }
    public string WaiterUserId { get; set; } = string.Empty;
    public long? GuestId { get; set; }
    public long? ReservationId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public RestaurantTables Table { get; set; } = null!;
    public ApplicationUser Waiter { get; set; } = null!;
    public Guests? Guest { get; set; }
    public Reservations? Reservation { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
