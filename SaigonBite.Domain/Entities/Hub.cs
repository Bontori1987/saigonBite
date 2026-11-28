using System.Collections.Generic;

namespace SaigonBite.Domain.Entities;

/// <summary>
/// A cloud kitchen. Minimal shape for UC1: enough to pick a hub, measure distance and read
/// capacity. UC2 (M2) owns the management side and will extend this.
///
/// Soft-delete per FR11: a hub is referenced by past orders and by report R2, so it is
/// deactivated rather than removed.
/// </summary>
public sealed class Hub
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;

    /// <summary>A-05: coordinates are stored so distance can be computed once and snapshotted.</summary>
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    /// <summary>
    /// Whole number of concurrent orders, 1 to 200 (FR8, A-06). The pricing engine receives
    /// this together with the active-order count and compares them as integers.
    /// </summary>
    public int KitchenCapacity { get; set; }

    /// <summary>Soft-delete flag — FR11. A deactivated hub stays in history and in R2.</summary>
    public bool IsActive { get; set; } = true;

    public List<Brand> Brands { get; set; } = new();
}
