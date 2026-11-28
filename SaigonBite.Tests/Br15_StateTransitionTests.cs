using SaigonBite.Domain.Orders;
using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR15 order lifecycle (FR46) and the BR14 cash guard on delivery (FR44).
///
/// These call the business layer directly, with no controller and no view, which is the
/// evidence that the rule lives in the domain rather than in a hidden button (NFR2a).
///
/// S-01 to S-06 are exactly the six rejections named in the FR46 Check in Part 1 §1.3.9.
/// S-06 is a role guard rather than a missing edge: the transition exists, but only for the
/// Shipper. S-07 is a condition guard: edge and actor are both right, and BR14 still blocks it.
/// </summary>
public class Br15_StateTransitionTests
{
    [Theory(DisplayName = "BR15 invalid transitions are rejected by the business layer")]
    [InlineData("S-01", OrderStatus.Delivered,      OrderStatus.Preparing, ActorRole.KitchenStaff)]
    [InlineData("S-02", OrderStatus.Cancelled,      OrderStatus.Confirmed, ActorRole.KitchenStaff)]
    [InlineData("S-03", OrderStatus.Preparing,      OrderStatus.Cancelled, ActorRole.Customer)]
    [InlineData("S-04", OrderStatus.Ready,          OrderStatus.Delivered, ActorRole.Shipper)]
    [InlineData("S-05", OrderStatus.Placed,         OrderStatus.Ready,     ActorRole.KitchenStaff)]
    [InlineData("S-06", OrderStatus.OutForDelivery, OrderStatus.Delivered, ActorRole.KitchenStaff)]
    public void Invalid_transition_is_rejected(
        string testId, OrderStatus from, OrderStatus to, ActorRole actor)
    {
        var order = new Order { Status = from, PaymentMethod = PaymentMethod.MoMo };

        var result = OrderStateMachine.Transition(order, to, actor);

        Assert.False(result.IsAllowed);
        Assert.Equal(from, order.Status);      // the order must not have moved
        Assert.Empty(order.StatusHistory);     // and no history row may be written
    }

    [Fact(DisplayName = "S-07 a COD order cannot be delivered before cash is confirmed")]
    public void S07_cod_requires_cash_confirmation()
    {
        var order = new Order
        {
            Status          = OrderStatus.OutForDelivery,
            PaymentMethod   = PaymentMethod.CashOnDelivery,
            RoundedTotal    = 205_000m,
            IsCashCollected = false
        };

        var result = OrderStateMachine.Transition(order, OrderStatus.Delivered, ActorRole.Shipper);

        Assert.False(result.IsAllowed);
        Assert.Equal(OrderStatus.OutForDelivery, order.Status);
        Assert.Empty(order.StatusHistory);
    }

    [Fact(DisplayName = "S-08 the same transition succeeds once cash is confirmed")]
    public void S08_cod_delivers_after_cash_is_confirmed()
    {
        var order = new Order
        {
            Status          = OrderStatus.OutForDelivery,
            PaymentMethod   = PaymentMethod.CashOnDelivery,
            RoundedTotal    = 205_000m,
            IsCashCollected = true
        };

        var result = OrderStateMachine.Transition(order, OrderStatus.Delivered, ActorRole.Shipper);

        Assert.True(result.IsAllowed);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Single(order.StatusHistory);    // exactly one history row (FR47)
    }

    [Fact(DisplayName = "S-09 an online-paid order is delivered without a cash confirmation")]
    public void S09_online_paid_order_needs_no_cash_confirmation()
    {
        var order = new Order
        {
            Status = OrderStatus.OutForDelivery,
            PaymentMethod = PaymentMethod.MoMo,
            RoundedTotal = 205_000m,
            IsCashCollected = false          // irrelevant for an online payment
        };

        var result = OrderStateMachine.Transition(order, OrderStatus.Delivered, ActorRole.Shipper);

        Assert.True(result.IsAllowed);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Single(order.StatusHistory);
    }
}
