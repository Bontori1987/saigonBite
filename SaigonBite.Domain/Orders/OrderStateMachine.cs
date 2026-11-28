using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Orders;

/// <summary>
/// BR15, enforced in the business layer rather than in the UI (FR46, NFR2a).
///
/// Three things are checked, in this order, and each one has its own test:
///   1. the edge exists in the table below        — S-01 to S-05
///   2. the actor is allowed on that edge         — S-06
///   3. the BR14 cash guard on Delivered          — S-07, S-08
///
/// Hiding a button in a Razor view satisfies none of these. The tests call this method
/// directly, with no controller involved, which is the evidence that the rule lives here.
/// </summary>
public static class OrderStateMachine
{
    private sealed record Edge(OrderStatus From, OrderStatus To, ActorRole[] Actors);

    /// <summary>
    /// Specification section 2.C, plus FR42 (an Admin may cancel a Confirmed order).
    /// Delivered, Cancelled and Failed appear only on the left of no edge at all: they are
    /// terminal, so they are simply absent as a From value.
    /// </summary>
    private static readonly Edge[] Allowed =
    {
        new(OrderStatus.Placed,         OrderStatus.Confirmed,
            new[] { ActorRole.KitchenStaff }),

        new(OrderStatus.Placed,         OrderStatus.Cancelled,
            new[] { ActorRole.Customer, ActorRole.KitchenStaff }),   // kitchen must give a reason

        new(OrderStatus.Confirmed,      OrderStatus.Preparing,
            new[] { ActorRole.KitchenStaff }),

        new(OrderStatus.Confirmed,      OrderStatus.Cancelled,
            new[] { ActorRole.Customer, ActorRole.Admin }),          // FR39, FR42

        new(OrderStatus.Preparing,      OrderStatus.Ready,
            new[] { ActorRole.KitchenStaff }),

        new(OrderStatus.Ready,          OrderStatus.OutForDelivery,
            new[] { ActorRole.Shipper }),                            // assigned shipper, FR43

        new(OrderStatus.OutForDelivery, OrderStatus.Delivered,
            new[] { ActorRole.Shipper }),                            // COD guard below, FR44

        new(OrderStatus.OutForDelivery, OrderStatus.Failed,
            new[] { ActorRole.Shipper })                             // FR45
    };

    public static TransitionResult Transition(
        Order order,
        OrderStatus target,
        ActorRole actor,
        string? reason = null)
    {
        var from = order.Status;

        // 1. Does the edge exist at all?
        Edge? edge = null;
        foreach (var candidate in Allowed)
        {
            if (candidate.From == from && candidate.To == target)
            {
                edge = candidate;
                break;
            }
        }

        if (edge is null)
            return TransitionResult.Deny(
                $"{from} cannot move to {target}: that transition is not in the order lifecycle.");

        // 2. Is this actor allowed on that edge?
        if (Array.IndexOf(edge.Actors, actor) < 0)
            return TransitionResult.Deny(
                $"{actor} may not move an order from {from} to {target}.");

        // 3. BR14: a COD order is not delivered until the cash is confirmed.
        if (target == OrderStatus.Delivered &&
            order.PaymentMethod == PaymentMethod.CashOnDelivery &&
            !order.IsCashCollected)
            return TransitionResult.Deny(
                "Cash on delivery: confirm the collected cash before marking the order delivered.");

        // Only now does anything change. A rejected call leaves both the status and the
        // history untouched, which is what the S-01 to S-07 assertions check.
        order.Status = target;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            FromStatus = from,
            ToStatus   = target,
            Actor      = actor,
            ChangedAt  = DateTime.Now,   // A-14: server time, Asia/Ho_Chi_Minh
            Reason     = reason
        });

        return TransitionResult.Allow();
    }
}
