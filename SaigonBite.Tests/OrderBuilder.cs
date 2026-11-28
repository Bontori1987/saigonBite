using System;
using System.Collections.Generic;
using System.Linq;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Tests;

/// <summary>
/// The baseline context from the test plan. Only the factor under test is overridden, so each
/// case stays a single-factor experiment and a failure points at one rule.
///
/// Baseline: 50 active orders of capacity 100 (load 50% — no Quiet Kitchen, hub open),
/// 1.5 km (band 1, D = 15,000), placed 15:00 (no rush), rain off, no voucher,
/// Local Customer paying by MoMo, one line.
/// </summary>
internal sealed class OrderBuilder
{
    private List<OrderLineInput> _lines = new() { new OrderLineInput(100_000m, 1) };
    private int _activeOrders  = 50;
    private int _capacity      = 100;
    private decimal _distance  = 1.5m;
    private DateTime _placedAt = new(2026, 11, 30, 15, 0, 0);
    private bool _rain         = false;
    private VoucherInput? _voucher = null;
    private CustomerType _customer = CustomerType.Local;
    private PaymentMethod _payment = PaymentMethod.MoMo;

    public OrderBuilder WithSubtotal(decimal s)
    {
        _lines = new List<OrderLineInput> { new(s, 1) };
        return this;
    }

    public OrderBuilder WithLines(params OrderLineInput[] lines)
    {
        _lines = lines.ToList();
        return this;
    }

    public OrderBuilder WithHubLoad(int activeOrders, int capacity = 100)
    {
        _activeOrders = activeOrders;
        _capacity = capacity;
        return this;
    }

    public OrderBuilder WithDistance(decimal km)
    {
        _distance = km;
        return this;
    }

    public OrderBuilder At(int hour, int minute)
    {
        // DateTimeKind.Unspecified on purpose: the engine reads TimeOfDay only, so these
        // tests give the same result whatever the machine time zone is set to (NFR6d).
        _placedAt = new DateTime(2026, 11, 30, hour, minute, 0);
        return this;
    }

    public OrderBuilder WithRain(bool on = true)
    {
        _rain = on;
        return this;
    }

    public OrderBuilder WithVoucher(VoucherInput voucher)
    {
        _voucher = voucher;
        return this;
    }

    public OrderBuilder AsLocal()
    {
        _customer = CustomerType.Local;
        return this;
    }

    public OrderBuilder AsTourist()
    {
        _customer = CustomerType.ForeignTourist;
        return this;
    }

    public OrderBuilder PayingBy(PaymentMethod method)
    {
        _payment = method;
        return this;
    }

    public PricingResult Calculate() => PricingEngine.Calculate(
        new PricingRequest(
            Lines:             _lines,
            ActiveOrdersAtHub: _activeOrders,
            HubCapacity:       _capacity,
            DistanceKm:        _distance,
            PlacedAt:          _placedAt,
            IsHeavyRain:       _rain,
            Voucher:           _voucher,
            CustomerType:      _customer,
            PaymentMethod:     _payment));

    public static VoucherInput Percent(decimal percent, decimal cap, decimal minSubtotal) =>
        new(VoucherType.Percent, percent, cap, minSubtotal, true,
            new DateOnly(2026, 1, 1), new DateOnly(2027, 12, 31));

    public static VoucherInput Fixed(decimal amount, decimal minSubtotal) =>
        new(VoucherType.Fixed, amount, 0m, minSubtotal, true,
            new DateOnly(2026, 1, 1), new DateOnly(2027, 12, 31));
}
