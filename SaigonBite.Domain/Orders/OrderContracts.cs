using System;
using System.Collections.Generic;
using SaigonBite.Domain.Entities;
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

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public ActorRole Actor { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// One line of an order. The item name and unit price are snapshotted at placement so editing
/// a menu item later never changes an existing order (FR16, BR17), and a soft-deleted item
/// still displays correctly in order history (FR17).
/// </summary>
public sealed class OrderLine
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    public string ItemNameSnapshot { get; set; } = string.Empty;

    /// <summary>FR16: "lines of an existing order keep 45,000".</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>1 to 20 — BR12, FR23. Also enforced by CK_ORDER_LINE_Quantity in the schema.</summary>
    public int Quantity { get; set; }

    /// <summary>Derived, never stored. Configured with Ignore in the DbContext.</summary>
    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>
/// An order through its whole life. Matches M2's ORDERS table, with the frozen conditions and
/// the nine price components held in <see cref="PriceSnapshot"/> rather than inline.
///
/// <see cref="RoundedTotal"/> is deliberately duplicated here as well as on the snapshot. M2's
/// schema does the same with ORDERS.Total: reports R1 and the order list read the total on
/// every row and should not have to join the snapshot to get it.
/// </summary>
public sealed class Order
{
    public int Id { get; set; }

    // ---- Who and where ------------------------------------------------------------------

    public int HubId { get; set; }
    public Hub? Hub { get; set; }

    /// <summary>
    /// BR13 needs this to decide which payment methods are offered. M2's schema keeps it on
    /// USERS; it is denormalised here so the pricing engine can be called without loading the
    /// customer, and so an order keeps the type the customer had at placement.
    /// </summary>
    public CustomerType CustomerType { get; set; }

    /// <summary>Placeholder until UC5 (M3) lands USERS, then this becomes CustomerId.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Placeholder until UC5 (M3) lands ADDRESS, then this becomes AddressId.</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    public List<OrderLine> Lines { get; set; } = new();

    // ---- Snapshot — BR17, FR36 ----------------------------------------------------------

    public PriceSnapshot? Snapshot { get; set; }

    // ---- Denormalised for listing and reports -------------------------------------------

    public DateTime PlacedAt { get; set; }

    public int? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }

    /// <summary>Same value as Snapshot.RoundedTotal — see the class remarks.</summary>
    public decimal RoundedTotal { get; set; }

    // ---- Payment -------------------------------------------------------------------------

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Set by the shipper when cash equal to RoundedTotal is collected — BR14, FR44.
    /// Not present in the schema on branch `DB`; the state machine's cash guard reads it, so
    /// ORDERS needs this column. To be confirmed with M2.
    /// </summary>
    public bool IsCashCollected { get; set; }

    public List<Payment> Payments { get; set; } = new();

    // ---- Lifecycle -------------------------------------------------------------------------

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    public List<OrderStatusHistory> StatusHistory { get; set; } = new();

    public string? CancelReason { get; set; }
    public string? DeliveryNote { get; set; }

    /// <summary>
    /// Store a calculated breakdown on the snapshot and mirror the total onto the order.
    /// </summary>
    public void ApplyBreakdown(PriceBreakdown breakdown)
    {
        Snapshot ??= new PriceSnapshot { OrderId = Id, Order = this };
        Snapshot.Apply(breakdown);
        RoundedTotal = breakdown.RoundedTotal;
    }

    /// <summary>
    /// Rebuild the pricing request from the stored snapshot, never from current conditions.
    /// FR38 uses this when an order is edited while Placed, and NFR6c's "re-price every stored
    /// order" check runs against it.
    /// </summary>
    public PricingRequest ToPricingRequestFromSnapshot(IReadOnlyList<OrderLineInput> lines)
    {
        if (Snapshot is null)
            throw new InvalidOperationException(
                $"Order {Id} has no price snapshot, so it cannot be re-priced (BR17).");

        return new PricingRequest(
            Lines:             lines,
            ActiveOrdersAtHub: Snapshot.ActiveOrdersAtHub,
            HubCapacity:       Snapshot.HubCapacity,
            DistanceKm:        Snapshot.DistanceKm,
            PlacedAt:          Snapshot.PlacedAt,
            IsHeavyRain:       Snapshot.IsHeavyRain,
            Voucher:           Voucher?.ToPricingInput(),
            CustomerType:      CustomerType,
            PaymentMethod:     PaymentMethod);
    }
}

public sealed record TransitionResult(bool IsAllowed, string? Reason)
{
    public static TransitionResult Allow()             => new(true, null);
    public static TransitionResult Deny(string reason) => new(false, reason);
}
