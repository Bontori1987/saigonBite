using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR4 rush-hour windows (FR29) and BR5 heavy rain (FR30).
/// The eight times are exactly the ones named in the FR29 Check in Part 1 §1.3.6.
/// Start inclusive, end exclusive: 13:00 and 19:30 must carry no surcharge.
/// </summary>
public class Br04_Br05_SurchargeTests
{
    [Theory(DisplayName = "BR4 rush-hour windows")]
    [InlineData("T-09", 10, 59,     0, 115_000)]
    [InlineData("T-10", 11,  0, 3_000, 118_000)]
    [InlineData("T-11", 12, 59, 3_000, 118_000)]
    [InlineData("T-12", 13,  0,     0, 115_000)]
    [InlineData("T-13", 17, 29,     0, 115_000)]
    [InlineData("T-14", 17, 30, 3_000, 118_000)]
    [InlineData("T-15", 19, 29, 3_000, 118_000)]
    [InlineData("T-16", 19, 30,     0, 115_000)]
    public void Rush_surcharge_respects_the_window_edges(
        string testId, int hour, int minute, int expectedRush, int expectedTotal)
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(100_000m)
            .At(hour, minute)
            .Calculate().Breakdown!;

        Assert.Equal(expectedRush,  breakdown.RushSurcharge);
        Assert.Equal(expectedTotal, breakdown.RoundedTotal);
    }

    [Fact(DisplayName = "T-17 the heavy-rain surcharge is a flat 10,000")]
    public void T17_rain_surcharge_is_flat()
    {
        var breakdown = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithRain()
            .Calculate().Breakdown!;

        Assert.Equal(10_000m,  breakdown.RainSurcharge);
        Assert.Equal(125_000m, breakdown.RoundedTotal);
    }
}
