using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR11 rounding to the nearest 1,000 with halves rounded up (FR31).
///
/// The four raw totals are the ones named in the FR31 Check in Part 1 §1.3.6 — 64,950 and
/// 64,500 round up to 65,000, just below the midpoint rounds down, and Q = 12,345.6 proves
/// that intermediate values are left unrounded. Each input is chosen so the Quiet Kitchen
/// discount produces the fractional value the Check asks for.
/// </summary>
public class Br11_RoundingTests
{
    [Theory(DisplayName = "BR11 rounding to nearest 1,000, half rounds up")]
    [InlineData("T-32", 55_500, 5_550.0, 64_950.0, 65_000)]
    [InlineData("T-33", 55_000, 5_500.0, 64_500.0, 65_000)]
    [InlineData("T-34", 54_999, 5_499.9, 64_499.1, 64_000)]
    public void Raw_total_rounds_half_up(
        string testId, int subtotal, double expectedQuiet,
        double expectedRaw, int expectedRounded)
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(subtotal)
            .WithHubLoad(29)
            .Calculate().Breakdown!;

        Assert.Equal((decimal)expectedQuiet, breakdown.QuietDiscount);
        Assert.Equal((decimal)expectedRaw,   breakdown.RawTotal);
        Assert.Equal(expectedRounded,        breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-35 intermediate values are never rounded")]
    public void T35_no_intermediate_rounding()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(123_456m)
            .WithHubLoad(29)
            .Calculate().Breakdown!;

        Assert.Equal( 12_345.6m, breakdown.QuietDiscount);   // not 12,346 and not 12,345
        Assert.Equal(111_110.4m, breakdown.FoodTotal);
        Assert.Equal(126_110.4m, breakdown.RawTotal);
        Assert.Equal(126_000m,   breakdown.RoundedTotal);
    }
}
