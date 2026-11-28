using System.Collections.Generic;

namespace SaigonBite.Domain.Entities;

/// <summary>
/// A virtual food brand cooked by one or more hubs (A-11: many-to-many).
/// Minimal shape for UC1 browsing. UC3 (M3) owns the management side.
///
/// Soft-delete per FR13: a brand is referenced by past order lines and by report R1.
/// </summary>
public sealed class Brand
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    /// <summary>Soft-delete flag — FR13.</summary>
    public bool IsDeleted { get; set; }

    public List<Hub> Hubs { get; set; } = new();
    public List<MenuItem> MenuItems { get; set; } = new();
}
