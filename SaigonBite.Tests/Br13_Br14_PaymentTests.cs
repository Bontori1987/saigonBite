using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR13 payment eligibility by customer group (FR33) and BR14 the cash limit (FR34).
///
/// T-47 and T-48 are the pair that matters: 1,000,400 and 1,000,500 straddle the rounding
/// midpoint, so together they prove the cash check reads the rounded total rather than the raw
/// one. An implementation that checked the raw total would still pass T-46.
/// </summary>
public class Br13_Br14_PaymentTests
{
    [Theory(DisplayName = "BR13 the payment method must match the customer group")]
    [InlineData("T-43", CustomerType.ForeignTourist, PaymentMethod.MoMo,   false,       0)]
    [InlineData("T-44", CustomerType.Local,          PaymentMethod.PayPal, false,       0)]
    [InlineData("T-45", CustomerType.ForeignTourist, PaymentMethod.PayPal, true,  115_000)]
    public void Payment_method_matches_the_customer_group(
        string testId, CustomerType customer, PaymentMethod method,
        bool expectAccepted, int expectedTotal)
    {
        var builder = new OrderBuilder().WithSubtotal(100_000m).PayingBy(method);

        var result = (customer == CustomerType.Local ? builder.AsLocal() : builder.AsTourist())
                     .Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);

        if (expectAccepted)
            Assert.Equal(expectedTotal, result.Breakdown!.RoundedTotal);
        else
            Assert.Equal(RejectionReason.PaymentMethodNotAllowed, result.Reason);
    }

    [Theory(DisplayName = "BR14 the COD limit reads the rounded total")]
    [InlineData("T-46", 1_000_000, 1_000_000, 1_000_000, true)]
    [InlineData("T-47", 1_000_400, 1_000_400, 1_000_000, true)]
    [InlineData("T-48", 1_000_500, 1_000_500, 1_001_000, false)]
    public void Cod_limit_boundary(
        string testId, int subtotal, int expectedRaw, int expectedRounded, bool expectAccepted)
    {
        var result = new OrderBuilder()
            .WithSubtotal(subtotal)
            .AsLocal()
            .PayingBy(PaymentMethod.CashOnDelivery)
            .Calculate();

        Assert.Equal(expectAccepted, result.IsAccepted);

        if (expectAccepted)
        {
            Assert.Equal(expectedRaw,     result.Breakdown!.RawTotal);
            Assert.Equal(expectedRounded, result.Breakdown!.RoundedTotal);
        }
        else
        {
            Assert.Equal(RejectionReason.CashLimitExceeded, result.Reason);
        }
    }

    [Fact(DisplayName = "T-49 the cash limit does not restrict non-cash payment")]
    public void T49_non_cod_is_unaffected()
    {
        var result = new OrderBuilder()
            .WithSubtotal(1_000_500m)
            .AsLocal()
            .PayingBy(PaymentMethod.MoMo)
            .Calculate();

        Assert.True(result.IsAccepted);
        Assert.Equal(1_001_000m, result.Breakdown!.RoundedTotal);
    }
}
