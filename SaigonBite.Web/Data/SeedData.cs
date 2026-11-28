using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SaigonBite.Domain.Entities;
using SaigonBite.Domain.Pricing;

namespace SaigonBite.Web.Data;

/// <summary>
/// Development seed data so UC1 and UC4 can be built and demonstrated before UC2 (hubs) and
/// UC3 (menu) exist. Three hubs in the districts named in the case study, three brands, twelve
/// menu items and three vouchers.
///
/// The hub loads are chosen so the three BR7/BR8 states can all be shown live in the demo:
/// District 1 is quiet (Q applies), Binh Thanh is normal, Thu Duc is full (orders refused).
///
/// This is not the NFR1 performance seed. That needs 10 hubs, 300 items and 1,000 orders and
/// is generated separately.
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeededAsync(SaigonBiteDbContext db)
    {
        if (await db.Hubs.AnyAsync())
            return;   // already seeded

        var phoExpress = new Brand
        {
            Name = "Pho Express",
            Description = "Northern-style pho and noodle soups"
        };

        var banhMiCorner = new Brand
        {
            Name = "Banh Mi Corner",
            Description = "Baguettes, rice plates and iced coffee"
        };

        var greenBowl = new Brand
        {
            Name = "Green Bowl",
            Description = "Salads, grain bowls and cold-pressed juice"
        };

        // A-11: a brand can run in several hubs; prices are the same everywhere.
        var hubs = new[]
        {
            new Hub
            {
                Name = "District 1 Hub",
                District = "District 1",
                StreetAddress = "12 Nguyen Hue",
                Latitude = 10.774000m,
                Longitude = 106.703000m,
                KitchenCapacity = 20,
                Brands = { phoExpress, banhMiCorner, greenBowl }
            },
            new Hub
            {
                Name = "Binh Thanh Hub",
                District = "Binh Thanh",
                StreetAddress = "88 Dien Bien Phu",
                Latitude = 10.804000m,
                Longitude = 106.713000m,
                KitchenCapacity = 15,
                Brands = { phoExpress, banhMiCorner }
            },
            new Hub
            {
                Name = "Thu Duc Hub",
                District = "Thu Duc",
                StreetAddress = "5 Vo Van Ngan",
                Latitude = 10.850000m,
                Longitude = 106.760000m,
                KitchenCapacity = 10,
                Brands = { greenBowl }
            }
        };

        var menuItems = new[]
        {
            Item(phoExpress,   "Beef pho",             "Pho bo",              "noodles",  65_000m),
            Item(phoExpress,   "Chicken pho",          "Pho ga",              "noodles",  58_000m),
            Item(phoExpress,   "Rare beef pho, large", "Pho tai, to lon",     "noodles",  79_000m),
            Item(phoExpress,   "Iced tea",             "Tra da",              "drinks",    8_000m),

            Item(banhMiCorner, "Grilled pork banh mi", "Banh mi thit nuong",  "banh mi",  35_000m),
            Item(banhMiCorner, "Cold cut banh mi",     "Banh mi thap cam",    "banh mi",  32_000m),
            Item(banhMiCorner, "Broken rice with pork","Com tam suon",        "rice",     55_000m),
            Item(banhMiCorner, "Vietnamese iced coffee","Ca phe sua da",      "drinks",   25_000m),

            Item(greenBowl,    "Chicken grain bowl",   "Bowl ga ngu coc",     "bowls",    89_000m),
            Item(greenBowl,    "Tofu salad bowl",      "Bowl salad dau hu",   "bowls",    79_000m),
            Item(greenBowl,    "Orange juice",         "Nuoc cam ep",         "drinks",   39_000m),

            // Soft-deleted on purpose: proves FR17 — absent from menus, still fine in history.
            Item(greenBowl,    "Seasonal mango bowl",  "Bowl xoai theo mua",  "bowls",    95_000m,
                 isAvailable: false, isDeleted: true)
        };

        var vouchers = new[]
        {
            new Voucher
            {
                Code = "SAIGON10",
                Type = VoucherType.Percent,
                Value = 10m,
                MaxDiscount = 30_000m,
                MinSubtotal = 100_000m,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2027, 12, 31),
                IsActive = true
            },
            new Voucher
            {
                Code = "FIXED50K",
                Type = VoucherType.Fixed,
                Value = 50_000m,
                MaxDiscount = null,
                MinSubtotal = 50_000m,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2027, 12, 31),
                IsActive = true
            },
            // Expired on purpose, so T-51's rejection path can be shown in the live demo.
            new Voucher
            {
                Code = "EXPIRED5",
                Type = VoucherType.Percent,
                Value = 5m,
                MaxDiscount = 20_000m,
                MinSubtotal = 50_000m,
                ValidFrom = new DateOnly(2026, 1, 1),
                ValidTo = new DateOnly(2026, 6, 30),
                IsActive = true
            }
        };

        db.Hubs.AddRange(hubs);
        db.MenuItems.AddRange(menuItems);
        db.Vouchers.AddRange(vouchers);

        await db.SaveChangesAsync();
    }

    private static MenuItem Item(
        Brand brand, string nameEn, string nameVi, string category, decimal price,
        bool isAvailable = true, bool isDeleted = false) =>
        new()
        {
            Brand = brand,
            NameEn = nameEn,
            NameVi = nameVi,
            Category = category,
            Price = price,
            IsAvailable = isAvailable,
            IsDeleted = isDeleted
        };
}
