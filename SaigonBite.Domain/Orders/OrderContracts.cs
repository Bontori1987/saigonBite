using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Orders;

public enum OrderStatus
{
    Placed, Confirmed, Preparing, Ready, OutForDelivery, Delivered, Cancelled, Failed
}

public enum ActorRole { Customer, KitchenStaff, Shipper, Admin }

/// <summary>One row per status change — FR47. Feeds R2's preparation time (FR50).</summary>
public sealed class OrderStatusHistory
{
    public int Id { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public ActorRole Actor { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Trimmed to what the state machine needs. The full entity (lines, snapshot, payment record,
/// assigned shipper) is added when the EF Core model lands; nothing here changes then.
/// </summary>
public sealed class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal RoundedTotal { get; set; }

    /// <summary>Set by the shipper when cash equal to RoundedTotal is collected — BR14.</summary>
    public bool IsCashCollected { get; set; }

    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
}

public sealed record TransitionResult(bool IsAllowed, string? Reason)
{
    public static TransitionResult Allow()              => new(true, null);
    public static TransitionResult Deny(string reason)  => new(false, reason);
}
