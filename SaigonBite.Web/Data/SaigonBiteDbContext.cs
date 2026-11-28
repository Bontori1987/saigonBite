using Microsoft.EntityFrameworkCore;
using SaigonBite.Domain.Entities;
using SaigonBite.Domain.Orders;

namespace SaigonBite.Web.Data;

/// <summary>
/// Code-First context for Tier 3.
///
/// Table and column names follow M2's schema on branch `DB`, so the two can be merged without
/// renaming anything once his entity classes arrive. Five tables are UC1 and UC4, owned by M1:
/// VOUCHER, ORDERS, ORDER_LINE, PRICE_SNAPSHOT, PAYMENT. HUB, BRAND and MENU_ITEM are present
/// in the minimal shape UC1 reads from; their management belongs to UC2 (M2) and UC3 (M3).
/// USERS, ADDRESS, VERIFICATION_CODE, AUDIT_LOG and SYSTEM_SETTING are not here yet.
///
/// Three things diverge from branch `DB` on purpose, each flagged for M2:
///  - PRICE_SNAPSHOT stores ActiveOrdersAtHub and HubCapacity as two integers rather than a
///    HubLoadPercent decimal, per A-06, so an order can be re-priced from its snapshot.
///  - ORDERS carries IsCashCollected, which the BR14 cash guard reads (FR44).
///  - Money stays decimal(18,2) per NFR6a rather than bigint.
///
/// Soft-delete: there are deliberately NO global query filters. A filter on Hub.IsActive or
/// MenuItem.IsDeleted would silently drop those rows out of order history and out of reports
/// R1 and R2 — which FR11, FR17 and FR49 require to stay visible. Filtering is done per query
/// instead, e.g. Hubs.Where(h =&gt; h.IsActive) when the customer picks a hub. State this choice
/// in section 3.2 of the report.
/// </summary>
public class SaigonBiteDbContext : DbContext
{
    public SaigonBiteDbContext(DbContextOptions<SaigonBiteDbContext> options) : base(options) { }

    public DbSet<Hub> Hubs => Set<Hub>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enums are stored as readable strings, matching branch `DB` (varchar columns there).
        // A number in the database would make the ORDERS table unreadable in SSMS during the
        // live demo, and would break silently if an enum member were ever reordered.

        // ---- HUB -------------------------------------------------------------------------

        modelBuilder.Entity<Hub>(hub =>
        {
            hub.ToTable("HUB");

            hub.Property(h => h.Name).IsRequired().HasMaxLength(100);
            hub.Property(h => h.District).IsRequired().HasMaxLength(50);
            hub.Property(h => h.StreetAddress).IsRequired().HasMaxLength(200);

            hub.Property(h => h.Latitude).HasPrecision(9, 6);
            hub.Property(h => h.Longitude).HasPrecision(9, 6);

            hub.HasIndex(h => h.Name).IsUnique();

            // FR8, A-06: a whole number of concurrent orders, 1 to 200.
            hub.ToTable(t => t.HasCheckConstraint(
                "CK_HUB_KitchenCapacity", "[KitchenCapacity] BETWEEN 1 AND 200"));

            // A-11: a brand can be operated by several hubs.
            hub.HasMany(h => h.Brands)
               .WithMany(b => b.Hubs)
               .UsingEntity(join => join.ToTable("HUB_BRAND"));
        });

        // ---- BRAND -----------------------------------------------------------------------

        modelBuilder.Entity<Brand>(brand =>
        {
            brand.ToTable("BRAND");

            brand.Property(b => b.Name).IsRequired().HasMaxLength(100);
            brand.Property(b => b.Description).HasMaxLength(500);
            brand.Property(b => b.LogoUrl).HasMaxLength(300);

            brand.HasMany(b => b.MenuItems)
                 .WithOne(m => m.Brand!)
                 .HasForeignKey(m => m.BrandId)
                 .OnDelete(DeleteBehavior.Restrict);   // soft-delete only, never cascade
        });

        // ---- MENU_ITEM -------------------------------------------------------------------

        modelBuilder.Entity<MenuItem>(item =>
        {
            item.ToTable("MENU_ITEM");

            item.Property(m => m.NameEn).IsRequired().HasMaxLength(100);
            item.Property(m => m.NameVi).IsRequired().HasMaxLength(100);
            item.Property(m => m.Category).IsRequired().HasMaxLength(50);
            item.Property(m => m.Description).HasMaxLength(500);
            item.Property(m => m.ImageUrl).HasMaxLength(300);

            item.Property(m => m.Price).HasPrecision(18, 2);   // NFR6a

            // FR14: a price must be greater than 0.
            item.ToTable(t => t.HasCheckConstraint("CK_MENU_ITEM_Price", "[Price] > 0"));
        });

        // ---- VOUCHER ---------------------------------------------------------------------

        modelBuilder.Entity<Voucher>(voucher =>
        {
            voucher.ToTable("VOUCHER");

            voucher.Property(v => v.Code).IsRequired().HasMaxLength(30);
            voucher.Property(v => v.Type).HasConversion<string>().HasMaxLength(10);

            // FR19: unique, case-insensitive. SQL Server's default collation is already
            // case-insensitive, so the unique index makes "SAIGON10" and "saigon10" collide.
            voucher.HasIndex(v => v.Code).IsUnique();

            voucher.Property(v => v.Value).HasPrecision(18, 2);
            voucher.Property(v => v.MaxDiscount).HasPrecision(18, 2);
            voucher.Property(v => v.MinSubtotal).HasPrecision(18, 2);

            // FR19, mirrored from branch `DB`. VoucherValidator enforces the same rules in the
            // business layer (T-54 to T-62); these are the database's last line of defence.
            voucher.ToTable(t =>
            {
                t.HasCheckConstraint("CK_VOUCHER_Dates", "[ValidTo] >= [ValidFrom]");
                t.HasCheckConstraint("CK_VOUCHER_Fixed", "[Type] <> 'Fixed' OR [Value] > 0");
                t.HasCheckConstraint("CK_VOUCHER_Percent",
                    "[Type] <> 'Percent' OR ([Value] BETWEEN 1 AND 100 AND [MaxDiscount] IS NOT NULL)");
            });
        });

        // ---- ORDERS ----------------------------------------------------------------------

        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("ORDERS");

            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(16);
            order.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(16);
            order.Property(o => o.CustomerType).HasConversion<string>().HasMaxLength(16);

            order.Property(o => o.CustomerName).HasMaxLength(100);
            order.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(200);
            order.Property(o => o.CancelReason).HasMaxLength(300);
            order.Property(o => o.DeliveryNote).HasMaxLength(300);

            order.Property(o => o.RoundedTotal).HasPrecision(18, 2);

            order.HasOne(o => o.Hub)
                 .WithMany()
                 .HasForeignKey(o => o.HubId)
                 .OnDelete(DeleteBehavior.Restrict);

            order.HasOne(o => o.Voucher)
                 .WithMany()
                 .HasForeignKey(o => o.VoucherId)
                 .OnDelete(DeleteBehavior.Restrict);

            order.HasMany(o => o.Lines)
                 .WithOne(l => l.Order!)
                 .HasForeignKey(l => l.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

            order.HasMany(o => o.StatusHistory)
                 .WithOne(h => h.Order!)
                 .HasForeignKey(h => h.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

            order.HasMany(o => o.Payments)
                 .WithOne(p => p.Order!)
                 .HasForeignKey(p => p.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

            order.HasOne(o => o.Snapshot)
                 .WithOne(s => s.Order!)
                 .HasForeignKey<PriceSnapshot>(s => s.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

            order.HasIndex(o => o.PlacedAt);   // R1 and R2 filter by date range
            order.HasIndex(o => o.Status);     // the kitchen queue filters by status
        });

        // ---- ORDER_LINE ------------------------------------------------------------------

        modelBuilder.Entity<OrderLine>(line =>
        {
            line.ToTable("ORDER_LINE");

            line.Property(l => l.ItemNameSnapshot).IsRequired().HasMaxLength(150);
            line.Property(l => l.UnitPrice).HasPrecision(18, 2);

            line.Ignore(l => l.LineTotal);   // derived, never stored

            // BR12, FR23. PricingEngine rejects the same values (T-36 to T-39).
            line.ToTable(t => t.HasCheckConstraint(
                "CK_ORDER_LINE_Quantity", "[Quantity] BETWEEN 1 AND 20"));

            line.HasOne(l => l.MenuItem)
                .WithMany()
                .HasForeignKey(l => l.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);   // FR17: soft-deleted, never removed
        });

        // ---- ORDER_STATUS_HISTORY --------------------------------------------------------

        modelBuilder.Entity<OrderStatusHistory>(history =>
        {
            history.ToTable("ORDER_STATUS_HISTORY");

            history.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(16);
            history.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(16);
            history.Property(h => h.Actor).HasConversion<string>().HasMaxLength(16);
            history.Property(h => h.Reason).HasMaxLength(300);

            history.HasIndex(h => new { h.OrderId, h.ChangedAt });
        });

        // ---- PRICE_SNAPSHOT --------------------------------------------------------------

        modelBuilder.Entity<PriceSnapshot>(snapshot =>
        {
            snapshot.ToTable("PRICE_SNAPSHOT");

            snapshot.HasKey(s => s.OrderId);   // one snapshot per order

            snapshot.Property(s => s.DistanceKm).HasPrecision(5, 2);   // A-05

            // Every money column to the same precision — NFR6a. Scale 2 is enough: the
            // smallest intermediate the engine can produce is a tenth of a dong (T-34).
            snapshot.Property(s => s.Subtotal).HasPrecision(18, 2);
            snapshot.Property(s => s.QuietDiscount).HasPrecision(18, 2);
            snapshot.Property(s => s.VoucherDiscount).HasPrecision(18, 2);
            snapshot.Property(s => s.FoodTotal).HasPrecision(18, 2);
            snapshot.Property(s => s.DeliveryFee).HasPrecision(18, 2);
            snapshot.Property(s => s.RushSurcharge).HasPrecision(18, 2);
            snapshot.Property(s => s.RainSurcharge).HasPrecision(18, 2);
            snapshot.Property(s => s.RawTotal).HasPrecision(18, 2);
            snapshot.Property(s => s.RoundedTotal).HasPrecision(18, 2);

            // A-06: capacity is bounded here too, so a snapshot can never divide by zero.
            snapshot.ToTable(t => t.HasCheckConstraint(
                "CK_PRICE_SNAPSHOT_Capacity", "[HubCapacity] BETWEEN 1 AND 200"));
        });

        // ---- PAYMENT ---------------------------------------------------------------------

        modelBuilder.Entity<Payment>(payment =>
        {
            payment.ToTable("PAYMENT");

            payment.Property(p => p.Method).HasConversion<string>().HasMaxLength(16);
            payment.Property(p => p.Kind).HasConversion<string>().HasMaxLength(10);
            payment.Property(p => p.Status).HasConversion<string>().HasMaxLength(10);
            payment.Property(p => p.GatewayRef).HasMaxLength(40);

            payment.Property(p => p.Amount).HasPrecision(18, 2);

            payment.HasIndex(p => p.OrderId);
        });
    }
}
