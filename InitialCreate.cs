using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonBite.Data.Migrations
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
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BRAND", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VOUCHER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Type = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDiscount = table.Column<long>(type: "bigint", nullable: true),
                    MinSubtotal = table.Column<long>(type: "bigint", nullable: false),
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
                    Price = table.Column<long>(type: "bigint", nullable: false),
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ADDRESS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StreetOrHotel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADDRESS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AUDIT_LOG",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    ActorRole = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Entity = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    EntityId = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Action = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIT_LOG", x => x.Id);
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeactivatedById = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HUB", x => x.Id);
                    table.CheckConstraint("CK_HUB_KitchenCapacity", "[KitchenCapacity] BETWEEN 1 AND 200");
                });

            migrationBuilder.CreateTable(
                name: "HUB_BRAND",
                columns: table => new
                {
                    HubId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HUB_BRAND", x => new { x.HubId, x.BrandId });
                    table.ForeignKey(
                        name: "FK_HUB_BRAND_BRAND_BrandId",
                        column: x => x.BrandId,
                        principalTable: "BRAND",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HUB_BRAND_HUB_HubId",
                        column: x => x.HubId,
                        principalTable: "HUB",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "varchar(12)", unicode: false, maxLength: 12, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserType = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    CustomerType = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: true),
                    PassportNumber = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    PhoneNumber = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: true),
                    HubId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.Id);
                    table.CheckConstraint("CK_USERS_KitchenStaffHub", "([Role] = 'KitchenStaff' AND [HubId] IS NOT NULL) OR ([Role] <> 'KitchenStaff' AND [HubId] IS NULL)");
                    table.CheckConstraint("CK_USERS_Phone", "[PhoneNumber] IS NULL OR ([PhoneNumber] LIKE '0[35789][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')");
                    table.ForeignKey(
                        name: "FK_USERS_HUB_HubId",
                        column: x => x.HubId,
                        principalTable: "HUB",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ORDERS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ShipperId = table.Column<int>(type: "int", nullable: true),
                    HubId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<long>(type: "bigint", nullable: false),
                    CancelReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DeliveryNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDERS_ADDRESS_AddressId",
                        column: x => x.AddressId,
                        principalTable: "ADDRESS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDERS_HUB_HubId",
                        column: x => x.HubId,
                        principalTable: "HUB",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDERS_USERS_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDERS_USERS_ShipperId",
                        column: x => x.ShipperId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDERS_VOUCHER_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "VOUCHER",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SYSTEM_SETTING",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSTEM_SETTING", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SYSTEM_SETTING_USERS_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VERIFICATION_CODE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attempts = table.Column<byte>(type: "tinyint", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VERIFICATION_CODE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VERIFICATION_CODE_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ORDER_LINE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<long>(type: "bigint", nullable: false),
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDER_LINE_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ORDER_STATUS_HISTORY",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    ToStatus = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_STATUS_HISTORY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ORDER_STATUS_HISTORY_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ORDER_STATUS_HISTORY_USERS_ActorId",
                        column: x => x.ActorId,
                        principalTable: "USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Kind = table.Column<string>(type: "varchar(7)", unicode: false, maxLength: 7, nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: false),
                    GatewayRef = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PAYMENT_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PRICE_SNAPSHOT",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HubLoadPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HeavyRain = table.Column<bool>(type: "bit", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuietDiscount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VoucherDiscount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FoodTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DeliveryFee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RushHourSurcharge = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RainSurcharge = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RawTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRICE_SNAPSHOT", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_PRICE_SNAPSHOT_ORDERS_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ORDERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ADDRESS_CustomerId",
                table: "ADDRESS",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_ActorId",
                table: "AUDIT_LOG",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOG_At",
                table: "AUDIT_LOG",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_BRAND_Name",
                table: "BRAND",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HUB_DeactivatedById",
                table: "HUB",
                column: "DeactivatedById");

            migrationBuilder.CreateIndex(
                name: "IX_HUB_Name",
                table: "HUB",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HUB_BRAND_BrandId",
                table: "HUB_BRAND",
                column: "BrandId");

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
                name: "IX_ORDER_STATUS_HISTORY_ActorId",
                table: "ORDER_STATUS_HISTORY",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_STATUS_HISTORY_OrderId_At",
                table: "ORDER_STATUS_HISTORY",
                columns: new[] { "OrderId", "At" });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_AddressId",
                table: "ORDERS",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_Code",
                table: "ORDERS",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_CustomerId_PlacedAt",
                table: "ORDERS",
                columns: new[] { "CustomerId", "PlacedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_HubId_Status",
                table: "ORDERS",
                columns: new[] { "HubId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_ShipperId",
                table: "ORDERS",
                column: "ShipperId");

            migrationBuilder.CreateIndex(
                name: "IX_ORDERS_VoucherId",
                table: "ORDERS",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_OrderId",
                table: "PAYMENT",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SYSTEM_SETTING_ChangedById",
                table: "SYSTEM_SETTING",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                table: "USERS",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_HubId",
                table: "USERS",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_PassportNumber_Nationality",
                table: "USERS",
                columns: new[] { "PassportNumber", "Nationality" },
                unique: true,
                filter: "[PassportNumber] IS NOT NULL AND [Nationality] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_PhoneNumber",
                table: "USERS",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VERIFICATION_CODE_UserId",
                table: "VERIFICATION_CODE",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VOUCHER_Code",
                table: "VOUCHER",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ADDRESS_USERS_CustomerId",
                table: "ADDRESS",
                column: "CustomerId",
                principalTable: "USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AUDIT_LOG_USERS_ActorId",
                table: "AUDIT_LOG",
                column: "ActorId",
                principalTable: "USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HUB_USERS_DeactivatedById",
                table: "HUB",
                column: "DeactivatedById",
                principalTable: "USERS",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HUB_USERS_DeactivatedById",
                table: "HUB");

            migrationBuilder.DropTable(
                name: "AUDIT_LOG");

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
                name: "SYSTEM_SETTING");

            migrationBuilder.DropTable(
                name: "VERIFICATION_CODE");

            migrationBuilder.DropTable(
                name: "MENU_ITEM");

            migrationBuilder.DropTable(
                name: "ORDERS");

            migrationBuilder.DropTable(
                name: "BRAND");

            migrationBuilder.DropTable(
                name: "ADDRESS");

            migrationBuilder.DropTable(
                name: "VOUCHER");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "HUB");
        }
    }
}
