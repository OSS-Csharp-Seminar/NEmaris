using NEmaris.Domain.Enums;

namespace NEmaris.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
}
