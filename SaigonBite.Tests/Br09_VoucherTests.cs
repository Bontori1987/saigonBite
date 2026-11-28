using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR9 voucher minimum subtotal, percentage cap and fixed clamping (FR32).
/// The voucher and the four subtotals are the ones named in the FR32 Check in Part 1 §1.3.6.
/// </summary>
public class Br09_VoucherTests
{
    private static VoucherInput TenPercent => OrderBuilder.Percent(10m, 30_000m, 100_000m);

    [Fact(DisplayName = "T-25 a voucher below its minimum subtotal is refused")]
    public void T25_minimum_subtotal_not_met()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(99_999m)
            .WithVoucher(TenPercent)
            .Calculate().Breakdown!;

        Assert.Equal(0m,       breakdown.VoucherDiscount);
        Assert.Equal(114_999m, breakdown.RawTotal);
        Assert.Equal(115_000m, breakdown.RoundedTotal);
    }

    [Theory(DisplayName = "BR9 percentage voucher value and cap")]
    [InlineData("T-26", 100_000, 10_000, 105_000)]  // under the cap
    [InlineData("T-27", 300_000, 30_000, 270_000)]  // exactly on the cap, D waived by BR6
    [InlineData("T-28", 400_000, 30_000, 370_000)]  // 40,000 clamped down to the cap
    public void Percent_voucher_is_capped(
        string testId, int subtotal, int expectedVoucher, int expectedTotal)
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(subtotal)
            .WithVoucher(TenPercent)
            .Calculate().Breakdown!;

        Assert.Equal(expectedVoucher, breakdown.VoucherDiscount);
        Assert.Equal(expectedTotal,   breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-29 a fixed voucher is clamped to (S - Q)")]
    public void T29_fixed_voucher_is_clamped()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(50_000m)
            .WithHubLoad(29)
            .WithVoucher(OrderBuilder.Fixed(50_000m, 50_000m))
            .Calculate().Breakdown!;

        Assert.Equal(5_000m,  breakdown.QuietDiscount);
        Assert.Equal(45_000m, breakdown.VoucherDiscount);   // not 50,000
        Assert.Equal(0m,      breakdown.FoodTotal);
        Assert.Equal(15_000m, breakdown.RoundedTotal);
    }
}
