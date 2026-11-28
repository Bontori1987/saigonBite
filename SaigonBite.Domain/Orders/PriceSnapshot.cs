using System;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Domain.Orders;

/// <summary>
/// The frozen conditions and the nine price components of one order — BR17, FR36.
/// One row per order, matching M2's PRICE_SNAPSHOT table.
///
/// Two deliberate differences from the schema on branch `DB`, both needed by the pricing
/// engine and both to be confirmed with M2:
///
/// 1. The hub load is stored as <see cref="ActiveOrdersAtHub"/> and <see cref="HubCapacity"/>,
///    two integers, rather than as a single HubLoadPercent decimal. Assumption A-06 says the
///    comparison is made in integers precisely so 30% and 90% are exact; a stored percentage
///    cannot be turned back into those two numbers, so re-pricing from the snapshot (NFR6c,
///    FR38) would not reproduce the original total.
///
/// 2. Money stays <c>decimal</c>, per NFR6a ("stored and computed as decimal, C# decimal,
///    SQL decimal(18,2)"). The schema on `DB` uses bigint for whole-dong columns, which is a
///    reasonable choice for VND but contradicts what Part 1 §1.4 commits to. One of the two
///    has to change: either the NFR text or the column types.
/// </summary>
public sealed class PriceSnapshot
{
    /// <summary>Primary key and foreign key at once — one snapshot per order.</summary>
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // ---- Conditions frozen at placement -------------------------------------------------

    /// <summary>A-14: server time, Asia/Ho_Chi_Minh, at the moment the order was saved.</summary>
    public DateTime PlacedAt { get; set; }

    /// <summary>A-06: orders in Confirmed or Preparing at that hub. Placed are not counted.</summary>
    public int ActiveOrdersAtHub { get; set; }

    /// <summary>A-06: the hub's kitchen capacity at that moment, 1 to 200.</summary>
    public int HubCapacity { get; set; }

    /// <summary>A-05: Haversine km rounded to 0.01.</summary>
    public decimal DistanceKm { get; set; }

    public bool IsHeavyRain { get; set; }

    // ---- The nine components of BR10 ----------------------------------------------------

    public decimal Subtotal { get; set; }         // S
    public decimal QuietDiscount { get; set; }    // Q  — BR7
    public decimal VoucherDiscount { get; set; }  // V  — BR9
    public decimal FoodTotal { get; set; }        // F  = S - Q - V
    public decimal DeliveryFee { get; set; }      // D  — BR3, waived by BR6
    public decimal RushSurcharge { get; set; }    // R  — BR4, waived by BR6
    public decimal RainSurcharge { get; set; }    // R2 — BR5
    public decimal RawTotal { get; set; }
    public decimal RoundedTotal { get; set; }     // BR11

    /// <summary>Copy a calculated breakdown in, so the nine columns can never go half-updated.</summary>
    public void Apply(PriceBreakdown breakdown)
    {
        Subtotal        = breakdown.Subtotal;
        QuietDiscount   = breakdown.QuietDiscount;
        VoucherDiscount = breakdown.VoucherDiscount;
        FoodTotal       = breakdown.FoodTotal;
        DeliveryFee     = breakdown.DeliveryFee;
        RushSurcharge   = breakdown.RushSurcharge;
        RainSurcharge   = breakdown.RainSurcharge;
        RawTotal        = breakdown.RawTotal;
        RoundedTotal    = breakdown.RoundedTotal;
    }
}
