using System;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Orders;

/// <summary>Whether a simulated charge or refund went through — FR35.</summary>
public enum PaymentStatus { Pending, Paid, Declined, Refunded }

/// <summary>A charge or a refund. Matches M2's PAYMENT table.</summary>
public enum PaymentKind { Charge, Refund }

/// <summary>
/// One payment record per attempt — FR35, FR39, FR45.
///
/// A COD order is created with <see cref="PaymentStatus.Pending"/> and only becomes Paid when
/// the shipper confirms the cash (BR14, FR44). An online order is created only after the mock
/// gateway approves, so its first record is already Paid; a cancellation or a failed delivery
/// adds a second record of kind <see cref="PaymentKind.Refund"/> rather than editing the first,
/// which is what keeps report R1 and the audit trail honest.
/// </summary>
public sealed class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentKind Kind { get; set; }

    /// <summary>Always positive. The kind says whether it moves money in or out.</summary>
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; }

    /// <summary>Reference returned by the simulated gateway — FR35.</summary>
    public string? GatewayRef { get; set; }

    public DateTime CreatedAt { get; set; }
}
