using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR6 free-delivery threshold (FR28). At 300,000 both D and R are waived, but R2 is not —
/// that exception is the point of T-20.
/// </summary>
public class Br06_FreeDeliveryTests
{
    [Fact(DisplayName = "T-18 one unit below the free-delivery threshold")]
    public void T18_fees_are_still_charged()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(299_999m)
            .At(11, 0)
            .Calculate().Breakdown!;

        Assert.Equal(15_000m,  breakdown.DeliveryFee);
        Assert.Equal(3_000m,   breakdown.RushSurcharge);
        Assert.Equal(317_999m, breakdown.RawTotal);
        Assert.Equal(318_000m, breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-19 at the threshold both D and R are waived")]
    public void T19_fees_are_waived()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(300_000m)
            .At(11, 0)
            .Calculate().Breakdown!;

        Assert.Equal(0m,       breakdown.DeliveryFee);
        Assert.Equal(0m,       breakdown.RushSurcharge);
        Assert.Equal(300_000m, breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-20 free delivery does not waive the rain surcharge")]
    public void T20_rain_survives_the_waiver()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(300_000m)
            .WithRain()
            .Calculate().Breakdown!;

        Assert.Equal(0m,       breakdown.DeliveryFee);
        Assert.Equal(0m,       breakdown.RushSurcharge);
        Assert.Equal(10_000m,  breakdown.RainSurcharge);
        Assert.Equal(310_000m, breakdown.RoundedTotal);
    }
}
