using System;
using System.Collections.Generic;
using System.Linq;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Vouchers;

/// <summary>Why a voucher could not be created or edited — FR19.</summary>
public enum VoucherRejection
{
    None,
    DuplicateCode,          // codes are unique, compared case-insensitively
    PercentOutOfRange,      // a PERCENT value must be 1 to 100
    PercentWithoutCap,      // a PERCENT voucher must carry a maximum discount
    FixedValueNotPositive,  // a FIXED amount must be greater than 0
    InvalidDateRange        // valid-to must not fall before valid-from
}

/// <summary>
/// What the admin typed on the UC4 create or edit form, before it becomes a
/// <see cref="Entities.Voucher"/>. A plain record so the rules can be tested without a database.
/// </summary>
public sealed record VoucherDraft(
    string Code,
    VoucherType Type,
    decimal Value,
    decimal? MaxDiscount,
    decimal MinSubtotal,
    DateOnly ValidFrom,
    DateOnly ValidTo);

/// <summary>
/// Creation-time rules for a voucher — UC4, FR19.
///
/// These are deliberately separate from BR9 in <see cref="PricingEngine"/>. The engine decides
/// whether an existing voucher applies to an order; this decides whether a voucher may exist at
/// all. The 1-to-100 range on a PERCENT value never reaches the engine, because by then the
/// voucher has already been saved.
///
/// A pure function: the caller passes the codes already in the database, so no DbContext is
/// needed and every rule is unit-testable.
/// </summary>
public static class VoucherValidator
{
    public const decimal MinPercent = 1m;
    public const decimal MaxPercent = 100m;

    /// <summary>
    /// Returns the first rule the draft breaks, or <see cref="VoucherRejection.None"/>.
    ///
    /// Order matters for the message the admin sees: identity first (is this code already
    /// taken), then the value rules for the chosen type, then the dates. A draft that breaks
    /// several rules reports the first one in that order.
    /// </summary>
    /// <param name="draft">The values entered on the form.</param>
    /// <param name="existingCodes">
    /// Codes already stored. When editing a voucher, exclude its own current code, otherwise it
    /// collides with itself.
    /// </param>
    public static VoucherRejection Validate(VoucherDraft draft, IEnumerable<string> existingCodes)
    {
        ArgumentNullException.ThrowIfNull(draft);
        ArgumentNullException.ThrowIfNull(existingCodes);

        // FR19 — unique code, case-insensitive. "SAIGON10" and "saigon10" are the same code.
        // Ordinal rather than culture-aware: a voucher code is an identifier, not prose, so it
        // must compare the same way on every machine regardless of its locale.
        if (existingCodes.Contains(draft.Code, StringComparer.OrdinalIgnoreCase))
            return VoucherRejection.DuplicateCode;

        switch (draft.Type)
        {
            case VoucherType.Percent:
                // FR19 — 1 to 100. Boundaries 0/1 and 100/101 are covered by T-54 to T-57.
                if (draft.Value < MinPercent || draft.Value > MaxPercent)
                    return VoucherRejection.PercentOutOfRange;

                // FR19 — a percentage without a ceiling could discount an unbounded amount,
                // so the cap is required. BR9 then clamps V to it.
                if (draft.MaxDiscount is null or <= 0m)
                    return VoucherRejection.PercentWithoutCap;

                break;

            case VoucherType.Fixed:
                // FR19 — amount greater than 0. MaxDiscount is ignored for FIXED, because BR9
                // already clamps the discount to (S - Q).
                if (draft.Value <= 0m)
                    return VoucherRejection.FixedValueNotPositive;

                break;
        }

        // FR19 — a window that ends before it starts can never be valid.
        // Equal dates are allowed: a one-day voucher runs 00:00:00 to 23:59:59 (A-09).
        if (draft.ValidTo < draft.ValidFrom)
            return VoucherRejection.InvalidDateRange;

        return VoucherRejection.None;
    }

    /// <summary>Convenience for the controller: true when the draft may be saved.</summary>
    public static bool IsValid(VoucherDraft draft, IEnumerable<string> existingCodes)
        => Validate(draft, existingCodes) == VoucherRejection.None;

    /// <summary>A message the admin can read, for the field-level error required by FR19.</summary>
    public static string DescribeRejection(VoucherRejection reason) => reason switch
    {
        VoucherRejection.None                  => string.Empty,
        VoucherRejection.DuplicateCode         => "That voucher code is already in use.",
        VoucherRejection.PercentOutOfRange     => "A percentage voucher must be between 1% and 100%.",
        VoucherRejection.PercentWithoutCap     => "A percentage voucher needs a maximum discount.",
        VoucherRejection.FixedValueNotPositive => "A fixed voucher must be worth more than 0 VND.",
        VoucherRejection.InvalidDateRange      => "The valid-to date cannot be before the valid-from date.",
        _                                      => "The voucher could not be saved."
    };
}
