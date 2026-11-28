using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>BR1 minimum order (FR24) and BR2 coverage distance (FR21).</summary>
public class Br01_Br02_EligibilityTests
{
    [Theory(DisplayName = "BR1 minimum order subtotal")]
    [InlineData("T-01", 49_999, false)]
    [InlineData("T-02", 50_000, true)]
    public void Subtotal_boundary_is_enforced(string testId, int subtotal, bool expectAccepted)
    {
        var result = new OrderBuilder().WithSubtotal(subtotal).Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);
        if (!expectAccepted)
            Assert.Equal(RejectionReason.BelowMinimumSubtotal, result.Reason);
    }

    [Fact(DisplayName = "T-02 total at the minimum subtotal")]
    public void T02_total_at_the_minimum()
    {
        var result = new OrderBuilder().WithSubtotal(50_000m).Calculate();

        Assert.Equal(65_000m, result.Breakdown!.RoundedTotal);
    }

    [Theory(DisplayName = "BR2 coverage boundary")]
    [InlineData("T-03", 8.00, true)]
    [InlineData("T-04", 8.01, false)]
    public void Coverage_boundary_is_enforced(string testId, double km, bool expectAccepted)
    {
        var result = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithDistance((decimal)km)
            .Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);
        if (!expectAccepted)
            Assert.Equal(RejectionReason.OutsideCoverage, result.Reason);
    }
}
