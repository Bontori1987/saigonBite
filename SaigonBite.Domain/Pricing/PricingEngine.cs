namespace SaigonBite.Domain.Pricing;

/// <summary>
/// The shared pricing module (BR1–BR14), owned by UC1 per Part 1 §1.1.4.
///
/// This is a pure function: no DbContext, no HttpContext, no clock of its own. Everything it
/// needs arrives in the PricingRequest, including the placement timestamp and the hub load, so
/// the same request always produces the same breakdown. That is what NFR6c ("re-pricing any
/// stored order from its snapshot reproduces its stored rounded total") depends on, and it is
/// why all 49 EP/BVA cases run without a database.
///
/// The nine steps of BR10 appear below in order and are numbered. Do not reorder them: T-31
/// fails if the voucher is taken on S rather than on (S - Q).
/// </summary>
public static class PricingEngine
{
    public static PricingResult Calculate(PricingRequest request)
    {
        // ---- Eligibility, before any money is computed ----------------------------------

        // BR12 — lines per order (FR23). Also catches the empty cart (T-42).
        if (request.Lines.Count < PricingPolicy.MinLinesPerOrder ||
            request.Lines.Count > PricingPolicy.MaxLinesPerOrder)
            return PricingResult.Reject(RejectionReason.InvalidLineCount);

        // BR12 — quantity per line (FR23).
        foreach (var line in request.Lines)
        {
            if (line.Quantity < PricingPolicy.MinQuantityPerLine ||
                line.Quantity > PricingPolicy.MaxQuantityPerLine)
                return PricingResult.Reject(RejectionReason.InvalidLineQuantity);
        }

        // (1) S = sum of price x quantity — BR10 step 1.
        decimal s = 0m;
        foreach (var line in request.Lines)
            s += line.UnitPrice * line.Quantity;

        // BR1 — minimum order (FR24).
        if (s < PricingPolicy.MinimumSubtotal)
            return PricingResult.Reject(RejectionReason.BelowMinimumSubtotal);

        // BR2 — coverage (FR21).
        if (request.DistanceKm > PricingPolicy.MaxDistanceKm)
            return PricingResult.Reject(RejectionReason.OutsideCoverage);

        // BR8 — hub load protection (FR22).
        if (IsHubFull(request.ActiveOrdersAtHub, request.HubCapacity))
            return PricingResult.Reject(RejectionReason.HubAtCapacity);

        // BR13 — payment method by customer group (FR33).
        if (!IsMethodAllowed(request.CustomerType, request.PaymentMethod))
            return PricingResult.Reject(RejectionReason.PaymentMethodNotAllowed);

        // ---- BR10, steps 2 to 9 ---------------------------------------------------------

        // (2) Q — Quiet Kitchen discount, BR7 (FR26).
        decimal q = IsQuietKitchen(request.ActiveOrdersAtHub, request.HubCapacity)
            ? s * PricingPolicy.QuietRate
            : 0m;

        // (3) V — voucher, BR9 (FR32). Taken on (S - Q), never on S.
        decimal v = VoucherDiscountFor(request.Voucher, s, q, request.PlacedAt);

        // (4) F — food total.
        decimal f = s - q - v;

        // (5) D — delivery fee, BR3, with the BR6 waiver (FR27, FR28).
        bool deliveryWaived = s >= PricingPolicy.FreeDeliveryFrom;
        decimal d = deliveryWaived ? 0m : DeliveryFeeFor(request.DistanceKm);

        // (6) R — rush-hour surcharge, BR4, only when D is not waived (FR29).
        decimal r = (!deliveryWaived && IsRushHour(request.PlacedAt))
            ? d * PricingPolicy.RushRate
            : 0m;

        // (7) R2 — heavy-rain surcharge, BR5. Survives the BR6 waiver (FR30, T-20).
        decimal r2 = request.IsHeavyRain ? PricingPolicy.RainSurcharge : 0m;

        // (8) Raw total. Nothing above this line has been rounded (T-35).
        decimal raw = f + d + r + r2;

        // (9) Round once, at the end — BR11 (FR31).
        decimal rounded = RoundToNearestThousand(raw);

        // BR14 — the cash limit reads the rounded total, not the raw total (FR34, T-47/T-48).
        if (request.PaymentMethod == PaymentMethod.CashOnDelivery &&
            rounded > PricingPolicy.CashLimit)
            return PricingResult.Reject(RejectionReason.CashLimitExceeded);

        return PricingResult.Accept(
            new PriceBreakdown(s, q, v, f, d, r, r2, raw, rounded));
    }

    // ---- BR3 ----------------------------------------------------------------------------

    /// <summary>
    /// "Per started km above" means Math.Ceiling, so 2.01 km already counts as one extra km.
    /// At 5.00 km both formulas agree on 30,000, which is why the bands join cleanly.
    /// </summary>
    private static decimal DeliveryFeeFor(decimal km)
    {
        if (km <= PricingPolicy.Band1LimitKm)
            return PricingPolicy.BaseDeliveryFee;

        if (km <= PricingPolicy.Band2LimitKm)
            return PricingPolicy.BaseDeliveryFee
                 + PricingPolicy.Band2PerKm * Math.Ceiling(km - PricingPolicy.Band1LimitKm);

        return PricingPolicy.Band3BaseFee
             + PricingPolicy.Band3PerKm * Math.Ceiling(km - PricingPolicy.Band2LimitKm);
    }

    // ---- BR4 ----------------------------------------------------------------------------

    /// <summary>
    /// Start inclusive, end exclusive. Using &lt;= on the right fails T-12 (13:00) and
    /// T-16 (19:30).
    /// </summary>
    private static bool IsRushHour(DateTime placedAt)
    {
        var t = placedAt.TimeOfDay;
        return (t >= PricingPolicy.LunchRushStart  && t < PricingPolicy.LunchRushEnd)
            || (t >= PricingPolicy.DinnerRushStart && t < PricingPolicy.DinnerRushEnd);
    }

    // ---- BR7 and BR8 ---------------------------------------------------------------------

    /// <summary>
    /// A-06: compare in integers. Dividing to get a percentage first would reintroduce the
    /// floating-point error at exactly 30% and 90% that the assumption was written to avoid.
    /// </summary>
    private static bool IsQuietKitchen(int activeOrders, int capacity)
        => activeOrders * 100 < PricingPolicy.QuietLoadPercent * capacity;

    private static bool IsHubFull(int activeOrders, int capacity)
        => activeOrders * 100 >= PricingPolicy.HubFullPercent * capacity;

    // ---- BR9 ----------------------------------------------------------------------------

    /// <summary>
    /// At most one voucher per order, so this takes a single voucher rather than a list.
    /// The minimum-subtotal condition is checked against S; the discount is computed on
    /// (S - Q). Validity dates are checked against the placement timestamp (A-09, A-14).
    /// V is never rounded here — BR11 rounds once, at the very end.
    /// </summary>
    private static decimal VoucherDiscountFor(
        VoucherInput? voucher, decimal s, decimal q, DateTime placedAt)
    {
        if (voucher is null || !voucher.IsActive)
            return 0m;

        var placedOn = DateOnly.FromDateTime(placedAt);
        if (placedOn < voucher.ValidFrom || placedOn > voucher.ValidTo)
            return 0m;

        if (s < voucher.MinSubtotal)
            return 0m;

        decimal basis = s - q;

        return voucher.Type switch
        {
            // Capped at the voucher's maximum discount (T-27 exactly on, T-28 above).
            VoucherType.Percent => Math.Min(basis * voucher.Value / 100m, voucher.MaxDiscount),

            // Never more than (S - Q), so F can reach 0 but never goes negative (T-29).
            VoucherType.Fixed   => Math.Min(voucher.Value, basis),

            _ => 0m
        };
    }

    // ---- BR11 ---------------------------------------------------------------------------

    /// <summary>
    /// Half rounds up. MidpointRounding.AwayFromZero is required: .NET's default is banker's
    /// rounding, which would turn 64,500 into 64,000 and fail T-33.
    /// </summary>
    private static decimal RoundToNearestThousand(decimal raw)
        => Math.Round(raw / PricingPolicy.RoundingStep, 0, MidpointRounding.AwayFromZero)
           * PricingPolicy.RoundingStep;

    // ---- BR13 ---------------------------------------------------------------------------

    /// <summary>
    /// Eligibility lives here rather than inside the Strategy classes, so adding a payment
    /// method stays "one new class plus one registration line" with nothing edited in the
    /// five existing ones (NFR2b).
    /// </summary>
    private static bool IsMethodAllowed(CustomerType customer, PaymentMethod method)
        => customer switch
        {
            CustomerType.Local => method is PaymentMethod.MoMo
                                         or PaymentMethod.VNPay
                                         or PaymentMethod.CashOnDelivery,

            CustomerType.ForeignTourist => method is PaymentMethod.ApplePay
                                                   or PaymentMethod.PayPal
                                                   or PaymentMethod.CashOnDelivery,

            _ => false
        };
}
