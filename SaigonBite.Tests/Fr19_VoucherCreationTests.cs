using System;
using SaigonBite.Domain.Pricing;
using SaigonBite.Domain.Vouchers;
using Xunit;

namespace SaigonBite.Tests;

/// <summary>
/// FR19 voucher creation rules (BR9) — UC4.
///
/// These cover every clause of the FR19 Check in Part 1 §1.3.4:
/// duplicate code "SAIGON10" vs "saigon10", PERCENT 0 or 101, valid-to before valid-from,
/// and PERCENT without a cap. The FIXED boundary is added because FR19 also requires an
/// amount greater than 0.
///
/// Note these run against VoucherValidator, not PricingEngine. The 1-to-100 range never
/// reaches the engine: by the time an order uses a voucher, it has already been saved.
/// </summary>
public class Fr19_VoucherCreationTests
{
    private static readonly DateOnly From = new(2026, 1, 1);
    private static readonly DateOnly To   = new(2027, 12, 31);

    private static VoucherDraft PercentDraft(decimal value, decimal? cap = 30_000m) =>
        new("SAIGON10", VoucherType.Percent, value, cap, 100_000m, From, To);

    private static VoucherDraft FixedDraft(decimal value) =>
        new("FIXED50K", VoucherType.Fixed, value, null, 50_000m, From, To);

    [Theory(DisplayName = "BR9 a PERCENT voucher value must be 1 to 100")]
    [InlineData("T-54",   0, VoucherRejection.PercentOutOfRange)]
    [InlineData("T-55",   1, VoucherRejection.None)]
    [InlineData("T-56", 100, VoucherRejection.None)]
    [InlineData("T-57", 101, VoucherRejection.PercentOutOfRange)]
    public void Percent_value_boundary(string testId, int value, VoucherRejection expected)
    {
        var result = VoucherValidator.Validate(PercentDraft(value), Array.Empty<string>());

        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "T-58 voucher codes are unique, whatever their case")]
    public void T58_duplicate_code_is_rejected()
    {
        var result = VoucherValidator.Validate(PercentDraft(10m), new[] { "saigon10" });

        Assert.Equal(VoucherRejection.DuplicateCode, result);
    }

    [Fact(DisplayName = "T-59 valid-to before valid-from is rejected")]
    public void T59_reversed_date_range_is_rejected()
    {
        var draft = new VoucherDraft(
            "SPRING26", VoucherType.Percent, 10m, 30_000m, 100_000m,
            ValidFrom: new DateOnly(2026, 6, 30),
            ValidTo:   new DateOnly(2026, 6, 1));

        var result = VoucherValidator.Validate(draft, Array.Empty<string>());

        Assert.Equal(VoucherRejection.InvalidDateRange, result);
    }

    [Fact(DisplayName = "T-60 a PERCENT voucher without a maximum discount is rejected")]
    public void T60_percent_without_cap_is_rejected()
    {
        var result = VoucherValidator.Validate(PercentDraft(10m, cap: null), Array.Empty<string>());

        Assert.Equal(VoucherRejection.PercentWithoutCap, result);
    }

    [Theory(DisplayName = "BR9 a FIXED voucher amount must be greater than 0")]
    [InlineData("T-61", 0, VoucherRejection.FixedValueNotPositive)]
    [InlineData("T-62", 1, VoucherRejection.None)]
    public void Fixed_value_boundary(string testId, int value, VoucherRejection expected)
    {
        var result = VoucherValidator.Validate(FixedDraft(value), Array.Empty<string>());

        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "FR19 a one-day voucher is valid (A-09)")]
    public void Single_day_window_is_allowed()
    {
        var sameDay = new DateOnly(2026, 11, 30);

        var draft = new VoucherDraft(
            "ONEDAY", VoucherType.Percent, 10m, 30_000m, 100_000m, sameDay, sameDay);

        var result = VoucherValidator.Validate(draft, Array.Empty<string>());

        Assert.Equal(VoucherRejection.None, result);
    }
}
