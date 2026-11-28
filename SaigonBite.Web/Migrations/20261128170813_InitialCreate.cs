using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BRAND",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BRAND", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HUB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    KitchenCapacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HUB", x => x.Id);
                    table.CheckConstraint("CK_HUB_KitchenCapacity", "[KitchenCapacity] BETWEEN 1 AND 200");
                });

            migrationBuilder.CreateTable(
                name: "VOUCHER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MinSubtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VOUCHER", x => x.Id);
                    table.CheckConstraint("CK_VOUCHER_Dates", "[ValidTo] >= [ValidFrom]");
                    table.CheckConstraint("CK_VOUCHER_Fixed", "[Type] <> 'Fixed' OR [Value] > 0");
                    table.CheckConstraint("CK_VOUCHER_Percent", "[Type] <> 'Percent' OR ([Value] BETWEEN 1 AND 100 AND [MaxDiscount] IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "MENU_ITEM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameVi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MENU_ITEM", x => x.Id);
                    table.CheckConstraint("CK_MENU_ITEM_Price", "[Price] > 0");
                    table.ForeignKey(
                        name: "FK_MENU_ITEM_BRAND_BrandId",
                        column: x => x.BrandId,
                        principalTable: "BRAND",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HUB_BRAND",
                columns: table => new
                {
                    BrandsId = table.Column<int>(type: "int", nullable: false),
                    HubsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HUB_BRAND", x => new { x.BrandsId, x.HubsId });
                    table.ForeignKey(
                        name: "FK_HUB_BRAND_BRAND_BrandsId",
                        column: x => x.BrandsId,
                        principalTable: "BRAND",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HUB_BRAND_HUB_HubsId",
                        column: x => x.HubsId,
                        principalTable: "HUB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HubId = table.Column<int>(type: "int", nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    RoundedTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IsCashCollected = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DeliveryNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDERS_HUB_HubId",
                        column: x => x.HubId,
                        principalTable: "HUB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ORDERS_VOUCHER_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "VOUCHER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_LINE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    ItemNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_LINE", x => x.Id);
                    table.CheckConstraint("CK_ORDER_LINE_Quantity", "[Quantity] BETWEEN 1 AND 20");
                    table.ForeignKey(
                        name: "FK_ORDER_LINE_MENU_ITEM_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MENU_ITEM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ORDER_LINE_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_STATUS_HISTORY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_STATUS_HISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDER_STATUS_HISTORY_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GatewayRef = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAYMENT_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRICE_SNAPSHOT",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveOrdersAtHub = table.Column<int>(type: "int", nullable: false),
                    HubCapacity = table.Column<int>(type: "int", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsHeavyRain = table.Column<bool>(type: "bit", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuietDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VoucherDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FoodTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RushSurcharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RainSurcharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RawTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RoundedTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRICE_SNAPSHOT", x => x.OrderId);
                    table.CheckConstraint("CK_PRICE_SNAPSHOT_Capacity", "[HubCapacity] BETWEEN 1 AND 200");
                    table.ForeignKey(
                        name: "FK_PRICE_SNAPSHOT_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HUB_Name",
                table: "HUB",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HUB_BRAND_HubsId",
                table: "HUB_BRAND",
                column: "HubsId");

            migrationBuilder.CreateIndex(
                name: "IX_MENU_ITEM_BrandId",
                table: "MENU_ITEM",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_LINE_MenuItemId",
                table: "ORDER_LINE",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_LINE_OrderId",
                table: "ORDER_LINE",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_STATUS_HISTORY_OrderId_ChangedAt",
                table: "ORDER_STATUS_HISTORY",
                columns: new[] { "OrderId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_HubId",
                table: "ORDERS",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_PlacedAt",
                table: "ORDERS",
                column: "PlacedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_Status",
                table: "ORDERS",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_VoucherId",
                table: "ORDERS",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_OrderId",
                table: "PAYMENT",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_VOUCHER_Code",
                table: "VOUCHER",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HUB_BRAND");

            migrationBuilder.DropTable(
                name: "ORDER_LINE");

            migrationBuilder.DropTable(
                name: "ORDER_STATUS_HISTORY");

            migrationBuilder.DropTable(
                name: "PAYMENT");

            migrationBuilder.DropTable(
                name: "PRICE_SNAPSHOT");

            migrationBuilder.DropTable(
                name: "MENU_ITEM");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "BRAND");

            migrationBuilder.DropTable(
                name: "HUB");

            migrationBuilder.DropTable(
                name: "VOUCHER");
        }
    }
}
