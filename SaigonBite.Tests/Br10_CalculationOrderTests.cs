using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR10 order of calculation (FR25).
///
/// T-30 reproduces the worked example from the specification, which Part 1 NFR6b names as a
/// mandatory check. T-31 is the discriminating case: most permutations of the nine steps give
/// the same number, but taking the voucher on S instead of (S - Q) does not.
/// </summary>
public class Br10_CalculationOrderTests
{
    [Fact(DisplayName = "T-30 specification worked example")]
    public void T30_worked_example()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(200_000m)
            .WithHubLoad(20)
            .WithDistance(3.2m)
            .At(15, 0)
            .PayingBy(PaymentMethod.CashOnDelivery)
            .Calculate().Breakdown!;

        Assert.Equal(200_000m, breakdown.Subtotal);
        Assert.Equal( 20_000m, breakdown.QuietDiscount);
        Assert.Equal(      0m, breakdown.VoucherDiscount);
        Assert.Equal(180_000m, breakdown.FoodTotal);
        Assert.Equal( 25_000m, breakdown.DeliveryFee);
        Assert.Equal(      0m, breakdown.RushSurcharge);
        Assert.Equal(      0m, breakdown.RainSurcharge);
        Assert.Equal(205_000m, breakdown.RawTotal);
        Assert.Equal(205_000m, breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-31 the percentage voucher is taken on (S - Q), not on S")]
    public void T31_voucher_base_is_s_minus_q()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(200_000m)
            .WithHubLoad(20)
            .WithVoucher(OrderBuilder.Percent(10m, 50_000m, 100_000m))
            .Calculate().Breakdown!;

        Assert.Equal( 20_000m, breakdown.QuietDiscount);
        Assert.Equal( 18_000m, breakdown.VoucherDiscount);  // 20,000 would mean the wrong base
        Assert.Equal(162_000m, breakdown.FoodTotal);
        Assert.Equal(177_000m, breakdown.RoundedTotal);     // 175,000 would mean the wrong base
    }
}
