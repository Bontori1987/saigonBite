using System.Diagnostics;
using SaigonBite.Domain.Pricing;
using Xunit;
using Xunit.Abstractions;

namespace SaigonBite.Tests;

/// <summary>
/// NFR1 (owner M1). Part 1 §1.4 promises: the pricing service returns the full price
/// breakdown for an order of 30 lines in under 1.5 s on 100% of 100 consecutive runs, with a
/// median of 200 ms or less, verified by "an automated timing test (xUnit + Stopwatch) in the
/// test project, output attached to the QA report".
///
/// Scope note for the report: this measures the pricing computation only. The NFR1 target also
/// covers the database reads for hub load, rain flag and voucher, which live in the controller
/// path and cannot be reached from a pure function. Part 1 already names the second half of
/// the verification — "spot check of the checkout request in the browser developer tools" —
/// so run that once UC1 checkout exists and attach both.
/// </summary>
public class Nfr1_PricingPerformanceTests
{
    private const int Runs          = 100;
    private const int LinesPerOrder = 30;      // the BR12 maximum, i.e. the worst case

    private const double MaxBudgetMs    = 1_500;
    private const double MedianBudgetMs = 200;

    private readonly ITestOutputHelper _output;

    public Nfr1_PricingPerformanceTests(ITestOutputHelper output) => _output = output;

    [Fact(DisplayName = "NFR1 a 30-line order is priced inside budget on 100 consecutive runs")]
    public void Pricing_meets_the_performance_target()
    {
        var request = BuildWorstCaseOrder();

        // Warm-up. The first calls pay for JIT compilation and would otherwise dominate the
        // maximum. NFR1 describes steady-state request handling, not cold start.
        for (var i = 0; i < 10; i++)
            PricingEngine.Calculate(request);

        var timingsMs = new List<double>(Runs);

        for (var i = 0; i < Runs; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = PricingEngine.Calculate(request);
            stopwatch.Stop();

            // A rejected order short-circuits before most of the work, so a silent rejection
            // would make the timings meaningless. Fail loudly instead.
            Assert.True(result.IsAccepted,
                $"the timing order was rejected: {result.Reason}");

            timingsMs.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        timingsMs.Sort();

        var median = Runs % 2 == 0
            ? (timingsMs[Runs / 2 - 1] + timingsMs[Runs / 2]) / 2
            : timingsMs[Runs / 2];

        var min = timingsMs[0];
        var max = timingsMs[^1];

        _output.WriteLine($"NFR1 — {Runs} consecutive runs, {LinesPerOrder} lines per order");
        _output.WriteLine($"  min    : {min:F4} ms");
        _output.WriteLine($"  median : {median:F4} ms   (target <= {MedianBudgetMs} ms)");
        _output.WriteLine($"  max    : {max:F4} ms   (target <  {MaxBudgetMs} ms)");
        _output.WriteLine($"  runs over budget: {timingsMs.Count(t => t >= MaxBudgetMs)} of {Runs}");

        Assert.True(max < MaxBudgetMs,
            $"slowest run was {max:F4} ms, the budget is {MaxBudgetMs} ms");

        Assert.True(median <= MedianBudgetMs,
            $"median was {median:F4} ms, the budget is {MedianBudgetMs} ms");
    }

    /// <summary>
    /// Built to take the longest path through the engine: 30 lines, the Quiet Kitchen discount,
    /// a voucher, a band-3 delivery fee, a rush-hour surcharge and the rain surcharge all apply.
    /// Subtotal stays under 300,000 on purpose, so the BR6 waiver does not skip the BR3 and BR4
    /// branches.
    /// </summary>
    private static PricingRequest BuildWorstCaseOrder()
    {
        var lines = Enumerable.Range(1, LinesPerOrder)
                              .Select(i => new OrderLineInput(2_000m + i * 100m, 2))
                              .ToList();   // S = 213,000

        var voucher = new VoucherInput(
            Type:        VoucherType.Percent,
            Value:       10m,
            MaxDiscount: 30_000m,
            MinSubtotal: 100_000m,
            IsActive:    true,
            ValidFrom:   new DateOnly(2026, 1, 1),
            ValidTo:     new DateOnly(2027, 12, 31));

        return new PricingRequest(
            Lines:             lines,
            ActiveOrdersAtHub: 25,                                  // below 30% → Q applies
            HubCapacity:       100,
            DistanceKm:        6.4m,                                // band 3
            PlacedAt:          new DateTime(2026, 11, 30, 12, 0, 0),// inside the lunch rush
            IsHeavyRain:       true,                                // R2 applies
            Voucher:           voucher,
            CustomerType:      CustomerType.Local,
            PaymentMethod:     PaymentMethod.MoMo);
    }
}
