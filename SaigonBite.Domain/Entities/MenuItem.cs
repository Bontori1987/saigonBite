namespace SaigonBite.Domain.Entities;

/// <summary>
/// One dish on a brand's menu. Minimal shape for UC1 browsing and ordering.
/// UC3 (M3) owns the management side and will extend this.
///
/// Soft-delete per FR17: a deleted item disappears from menus and search but must still show
/// in past orders and count in report R1.
/// </summary>
public sealed class MenuItem
{
    public int Id { get; set; }

    public int BrandId { get; set; }
    public Brand? Brand { get; set; }

    /// <summary>NFR3b: every customer-facing label exists in both languages.</summary>
    public string NameEn { get; set; } = string.Empty;
    public string NameVi { get; set; } = string.Empty;

    /// <summary>From an admin-maintained list, e.g. noodles, rice, drinks (FR14).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Whole VND greater than 0 (FR14). Validated on the UC3 create/edit path.</summary>
    public decimal Price { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>FR18: an unavailable item is shown greyed out with no Add button.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Soft-delete flag — FR17.</summary>
    public bool IsDeleted { get; set; }
}
