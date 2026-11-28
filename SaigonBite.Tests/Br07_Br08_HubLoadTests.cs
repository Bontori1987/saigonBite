using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR7 Quiet Kitchen (FR26) and BR8 hub protection (FR22).
/// Load is expressed as active orders against a capacity of 100, per assumption A-06: the
/// engine compares integers rather than a percentage, so the 30% and 90% edges are exact.
/// </summary>
public class Br07_Br08_HubLoadTests
{
    [Theory(DisplayName = "BR7 Quiet Kitchen threshold")]
    [InlineData("T-21", 29, 10_000, 105_000)]
    [InlineData("T-22", 30,      0, 115_000)]
    public void Quiet_kitchen_discount_at_the_threshold(
        string testId, int activeOrders, int expectedQuiet, int expectedTotal)
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithHubLoad(activeOrders, capacity: 100)
            .Calculate().Breakdown!;

        Assert.Equal(expectedQuiet, breakdown.QuietDiscount);
        Assert.Equal(expectedTotal, breakdown.RoundedTotal);
    }

    [Theory(DisplayName = "BR8 hub load protection")]
    [InlineData("T-23", 89, true)]
    [InlineData("T-24", 90, false)]
    public void Hub_refuses_new_orders_at_capacity(
        string testId, int activeOrders, bool expectAccepted)
    {
        var result = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithHubLoad(activeOrders, capacity: 100)
            .Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);

        if (expectAccepted)
            Assert.Equal(115_000m, result.Breakdown!.RoundedTotal);
        else
            Assert.Equal(RejectionReason.HubAtCapacity, result.Reason);
    }
}
