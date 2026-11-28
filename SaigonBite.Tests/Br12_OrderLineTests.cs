using System.Linq;
using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR12 quantity per line, 1 to 20, and lines per order, 1 to 30 (FR23).
///
/// The quantity cases carry an anchor line of 50,000 so the order clears the BR1 minimum on
/// its own: without it, a quantity of 0 or 1 on a 5,000 item would be rejected for the wrong
/// reason and the test would pass while proving nothing.
/// </summary>
public class Br12_OrderLineTests
{
    private static OrderLineInput Anchor => new(50_000m, 1);

    [Theory(DisplayName = "BR12 quantity per line, 1 to 20")]
    [InlineData("T-36",  0, false,       0)]
    [InlineData("T-37",  1, true,   70_000)]
    [InlineData("T-38", 20, true,  165_000)]
    [InlineData("T-39", 21, false,       0)]
    public void Line_quantity_boundary(
        string testId, int quantity, bool expectAccepted, int expectedTotal)
    {
        var result = new OrderBuilder()
            .WithLines(Anchor, new OrderLineInput(5_000m, quantity))
            .Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);

        if (expectAccepted)
            Assert.Equal(expectedTotal, result.Breakdown!.RoundedTotal);
        else
            Assert.Equal(RejectionReason.InvalidLineQuantity, result.Reason);
    }

    [Theory(DisplayName = "BR12 lines per order, 1 to 30")]
    [InlineData("T-40", 30, true,  165_000)]
    [InlineData("T-41", 31, false,       0)]
    [InlineData("T-42",  0, false,       0)]   // an empty cart cannot check out
    public void Line_count_boundary(
        string testId, int lineCount, bool expectAccepted, int expectedTotal)
    {
        var lines = Enumerable.Range(0, lineCount)
                              .Select(_ => new OrderLineInput(5_000m, 1))
                              .ToArray();

        var result = new OrderBuilder().WithLines(lines).Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);

        if (expectAccepted)
            Assert.Equal(expectedTotal, result.Breakdown!.RoundedTotal);
        else
            Assert.Equal(RejectionReason.InvalidLineCount, result.Reason);
    }
}
