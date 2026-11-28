using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR3 delivery fee bands (FR27). The values are exactly the ones named in the FR27 Check in
/// Part 1 §1.3.6: 2.00 = 15,000; 2.01 = 20,000; 5.00 = 30,000; 5.01 = 37,000; 8.00 = 51,000.
/// </summary>
public class Br03_DeliveryFeeTests
{
    [Theory(DisplayName = "BR3 delivery fee bands")]
    [InlineData("T-05", 2.00, 15_000, 115_000)]
    [InlineData("T-06", 2.01, 20_000, 120_000)]
    [InlineData("T-07", 5.00, 30_000, 130_000)]
    [InlineData("T-08", 5.01, 37_000, 137_000)]
    [InlineData("T-03", 8.00, 51_000, 151_000)]
    public void Delivery_fee_matches_its_band(
        string testId, double km, int expectedFee, int expectedTotal)
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithDistance((decimal)km)
            .Calculate().Breakdown!;

        Assert.Equal(expectedFee,   breakdown.DeliveryFee);
        Assert.Equal(expectedTotal, breakdown.RoundedTotal);
    }
}
