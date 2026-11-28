using System;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Entities;

/// <summary>
/// A discount voucher — UC4, owned by M1. BR9, FR19, FR20.
///
/// This is the persisted entity. <see cref="VoucherInput"/> in the Pricing namespace is the
/// value the pricing engine consumes; <see cref="ToPricingInput"/> bridges the two. Keeping
/// them apart is what lets the engine stay free of any database type.
/// </summary>
public sealed class Voucher
{
    public int Id { get; set; }

    /// <summary>Unique, compared case-insensitively — FR19 (A-09).</summary>
    public string Code { get; set; } = string.Empty;

    public VoucherType Type { get; set; }

    /// <summary>Percentage 1 to 100 for PERCENT, or a VND amount greater than 0 for FIXED.</summary>
    public decimal Value { get; set; }

    /// <summary>Required for PERCENT, unused for FIXED — FR19.</summary>
    public decimal? MaxDiscount { get; set; }

    /// <summary>Checked against S, never against (S - Q) — BR9.</summary>
    public decimal MinSubtotal { get; set; }

    /// <summary>A-09: valid from 00:00:00 on ValidFrom to 23:59:59 on ValidTo, Vietnam time.</summary>
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidTo { get; set; }

    /// <summary>
    /// FR20: a deactivated voucher can no longer be applied, but orders that already used it
    /// keep their stored discount. That is why this is not a hard delete.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Hand this to PricingEngine.Calculate. BR9 validity is re-checked there.</summary>
    public VoucherInput ToPricingInput() => new(
        Type:        Type,
        Value:       Value,
        MaxDiscount: MaxDiscount ?? 0m,
        MinSubtotal: MinSubtotal,
        IsActive:    IsActive,
        ValidFrom:   ValidFrom,
        ValidTo:     ValidTo);
}
