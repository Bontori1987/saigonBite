namespace SaigonBite.Domain.Pricing;

public enum PaymentMethod { MoMo, VNPay, ApplePay, PayPal, CashOnDelivery }

public enum CustomerType { Local, ForeignTourist }

public enum VoucherType { Percent, Fixed }

public enum RejectionReason
{
    None,
    BelowMinimumSubtotal,     // BR1  — FR24
    OutsideCoverage,          // BR2  — FR21
    HubAtCapacity,            // BR8  — FR22
    InvalidLineQuantity,      // BR12 — FR23
    InvalidLineCount,         // BR12 — FR23
    PaymentMethodNotAllowed,  // BR13 — FR33
    CashLimitExceeded         // BR14 — FR34
}

/// <summary>
/// Every rule threshold lives here exactly once (NFR2a). A search for a literal such as
/// 50000 or 300000 anywhere outside this class should return no hits — that search is the
/// evidence cited in the NFR2 verification column of Part 1 §1.4.
/// </summary>
public static class PricingPolicy
{
    public const decimal MinimumSubtotal  = 50_000m;    // BR1
    public const decimal MaxDistanceKm    = 8.0m;       // BR2
    public const decimal BaseDeliveryFee  = 15_000m;    // BR3 band 1
    public const decimal Band1LimitKm     = 2.0m;       // BR3
    public const decimal Band2LimitKm     = 5.0m;       // BR3
    public const decimal Band2PerKm       = 5_000m;     // BR3
    public const decimal Band3BaseFee     = 30_000m;    // BR3
    public const decimal Band3PerKm       = 7_000m;     // BR3
    public const decimal RushRate         = 0.20m;      // BR4
    public const decimal RainSurcharge    = 10_000m;    // BR5
    public const decimal FreeDeliveryFrom = 300_000m;   // BR6
    public const int     QuietLoadPercent = 30;         // BR7
    public const decimal QuietRate        = 0.10m;      // BR7
    public const int     HubFullPercent   = 90;         // BR8
    public const decimal RoundingStep     = 1_000m;     // BR11
    public const int     MinQuantityPerLine = 1;        // BR12
    public const int     MaxQuantityPerLine = 20;       // BR12
    public const int     MinLinesPerOrder   = 1;        // BR12
    public const int     MaxLinesPerOrder   = 30;       // BR12
    public const decimal CashLimit        = 1_000_000m; // BR14

    public static readonly TimeSpan LunchRushStart  = new(11, 0, 0);   // BR4
    public static readonly TimeSpan LunchRushEnd    = new(13, 0, 0);   // BR4
    public static readonly TimeSpan DinnerRushStart = new(17, 30, 0);  // BR4
    public static readonly TimeSpan DinnerRushEnd   = new(19, 30, 0);  // BR4
}

public sealed record OrderLineInput(decimal UnitPrice, int Quantity);

public sealed record VoucherInput(
    VoucherType Type,
    decimal Value,          // percent (e.g. 10m) or fixed amount
    decimal MaxDiscount,    // ignored for Fixed
    decimal MinSubtotal,
    bool IsActive,
    DateOnly ValidFrom,
    DateOnly ValidTo);

public sealed record PricingRequest(
    IReadOnlyList<OrderLineInput> Lines,
    int ActiveOrdersAtHub,   // A-06: integers, never a pre-computed percentage
    int HubCapacity,         // A-06: whole number, 1 to 200 (FR8)
    decimal DistanceKm,      // A-05: Haversine km rounded to 0.01
    DateTime PlacedAt,       // A-14: server time, Asia/Ho_Chi_Minh
    bool IsHeavyRain,
    VoucherInput? Voucher,
    CustomerType CustomerType,
    PaymentMethod PaymentMethod);

public sealed record PriceBreakdown(
    decimal Subtotal,         // S
    decimal QuietDiscount,    // Q  — BR7
    decimal VoucherDiscount,  // V  — BR9
    decimal FoodTotal,        // F  = S - Q - V
    decimal DeliveryFee,      // D  — BR3, waived by BR6
    decimal RushSurcharge,    // R  — BR4, waived by BR6
    decimal RainSurcharge,    // R2 — BR5
    decimal RawTotal,         // F + D + R + R2
    decimal RoundedTotal);    // BR11

public sealed record PricingResult(
    bool IsAccepted,
    RejectionReason Reason,
    PriceBreakdown? Breakdown)
{
    public static PricingResult Reject(RejectionReason reason) => new(false, reason, null);
    public static PricingResult Accept(PriceBreakdown b)       => new(true, RejectionReason.None, b);
}
