using System;
using SaigonBite.Domain.Pricing;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// BR9 validity window and active flag (FR32, FR20), following assumption A-09: a voucher is
/// valid from 00:00:00 on its start date to 23:59:59 on its end date, checked against the
/// placement timestamp.
///
/// The baseline places the order on 30 Nov 2026, so these move the voucher's own dates around
/// that fixed point rather than moving the order.
/// </summary>
public class Br09_VoucherValidityTests
{
    private static VoucherInput Voucher(DateOnly validFrom, DateOnly validTo, bool isActive) =>
        new(VoucherType.Percent, 10m, 30_000m, 100_000m, isActive, validFrom, validTo);

    [Theory(DisplayName = "BR9 voucher validity window and active flag")]
    [InlineData("T-50", 2026,  1, 1, 2026, 11, 30, true,  10_000, 105_000)]
    [InlineData("T-51", 2026,  1, 1, 2026, 11, 29, true,       0, 115_000)]
    [InlineData("T-52", 2026, 12, 1, 2027, 12, 31, true,       0, 115_000)]
    [InlineData("T-53", 2026,  1, 1, 2027, 12, 31, false,      0, 115_000)]
    public void Voucher_applies_only_inside_its_window(
        string testId,
        int fromYear, int fromMonth, int fromDay,
        int toYear,   int toMonth,   int toDay,
        bool isActive, int expectedVoucher, int expectedTotal)
    {
        var voucher = Voucher(
            new DateOnly(fromYear, fromMonth, fromDay),
            new DateOnly(toYear, toMonth, toDay),
            isActive);

        var breakdown = new OrderBuilder()
            .WithSubtotal(100_000m)
            .WithVoucher(voucher)
            .Calculate().Breakdown!;

        Assert.Equal(expectedVoucher, breakdown.VoucherDiscount);
        Assert.Equal(expectedTotal,   breakdown.RoundedTotal);
    }
}
