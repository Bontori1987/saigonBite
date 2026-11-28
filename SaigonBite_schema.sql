-- SaigonBite schema: every EF Core migration as one idempotent SQL script (safe to run more than once).
-- Regenerate: dotnet ef migrations script --idempotent --project src/SaigonBite.Data --startup-project src/SaigonBite.Web -o db/SaigonBite_schema.sql
-- Filtered unique indexes need these two options ON (sqlcmd turns QUOTED_IDENTIFIER off unless you pass -I).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [BRAND] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [LogoUrl] nvarchar(300) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_BRAND] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [VOUCHER] (
        [Id] int NOT NULL IDENTITY,
        [Code] varchar(30) NOT NULL,
        [Type] varchar(7) NOT NULL,
        [Value] decimal(18,2) NOT NULL,
        [MaxDiscount] bigint NULL,
        [MinSubtotal] bigint NOT NULL,
        [ValidFrom] date NOT NULL,
        [ValidTo] date NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_VOUCHER] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_VOUCHER_Dates] CHECK ([ValidTo] >= [ValidFrom]),
        CONSTRAINT [CK_VOUCHER_Fixed] CHECK ([Type] <> 'Fixed' OR [Value] > 0),
        CONSTRAINT [CK_VOUCHER_Percent] CHECK ([Type] <> 'Percent' OR ([Value] BETWEEN 1 AND 100 AND [MaxDiscount] IS NOT NULL))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [MENU_ITEM] (
        [Id] int NOT NULL IDENTITY,
        [BrandId] int NOT NULL,
        [NameEn] nvarchar(100) NOT NULL,
        [NameVi] nvarchar(100) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Price] bigint NOT NULL,
        [ImageUrl] nvarchar(300) NULL,
        [IsAvailable] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MENU_ITEM] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MENU_ITEM_Price] CHECK ([Price] > 0),
        CONSTRAINT [FK_MENU_ITEM_BRAND_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [BRAND] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [ADDRESS] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Label] nvarchar(50) NOT NULL,
        [StreetOrHotel] nvarchar(200) NOT NULL,
        [RoomNumber] nvarchar(10) NULL,
        [Latitude] decimal(9,6) NOT NULL,
        [Longitude] decimal(9,6) NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ADDRESS] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [AUDIT_LOG] (
        [Id] bigint NOT NULL IDENTITY,
        [ActorId] int NULL,
        [ActorRole] varchar(12) NOT NULL,
        [At] datetime2 NOT NULL,
        [Entity] varchar(40) NOT NULL,
        [EntityId] varchar(40) NOT NULL,
        [Action] varchar(20) NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [Reason] nvarchar(300) NULL,
        CONSTRAINT [PK_AUDIT_LOG] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [HUB] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [District] nvarchar(50) NOT NULL,
        [StreetAddress] nvarchar(200) NOT NULL,
        [Latitude] decimal(9,6) NOT NULL,
        [Longitude] decimal(9,6) NOT NULL,
        [KitchenCapacity] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DeactivatedAt] datetime2 NULL,
        [DeactivatedById] int NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_HUB] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HUB_KitchenCapacity] CHECK ([KitchenCapacity] BETWEEN 1 AND 200)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [HUB_BRAND] (
        [HubId] int NOT NULL,
        [BrandId] int NOT NULL,
        CONSTRAINT [PK_HUB_BRAND] PRIMARY KEY ([HubId], [BrandId]),
        CONSTRAINT [FK_HUB_BRAND_BRAND_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [BRAND] ([Id]),
        CONSTRAINT [FK_HUB_BRAND_HUB_HubId] FOREIGN KEY ([HubId]) REFERENCES [HUB] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [USERS] (
        [Id] int NOT NULL IDENTITY,
        [Role] varchar(12) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NULL,
        [PasswordHash] nvarchar(200) NOT NULL,
        [IsVerified] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [DeactivatedAt] datetime2 NULL,
        [UserType] varchar(7) NOT NULL,
        [CustomerType] varchar(7) NULL,
        [PassportNumber] varchar(9) NULL,
        [Nationality] nvarchar(60) NULL,
        [PhoneNumber] char(10) NULL,
        [HubId] int NULL,
        CONSTRAINT [PK_USERS] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_USERS_KitchenStaffHub] CHECK (([Role] = 'KitchenStaff' AND [HubId] IS NOT NULL) OR ([Role] <> 'KitchenStaff' AND [HubId] IS NULL)),
        CONSTRAINT [CK_USERS_Phone] CHECK ([PhoneNumber] IS NULL OR ([PhoneNumber] LIKE '0[35789][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]')),
        CONSTRAINT [FK_USERS_HUB_HubId] FOREIGN KEY ([HubId]) REFERENCES [HUB] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [ORDERS] (
        [Id] int NOT NULL IDENTITY,
        [Code] varchar(20) NOT NULL,
        [CustomerId] int NOT NULL,
        [ShipperId] int NULL,
        [HubId] int NOT NULL,
        [AddressId] int NOT NULL,
        [VoucherId] int NULL,
        [Status] varchar(16) NOT NULL,
        [PaymentMethod] varchar(10) NOT NULL,
        [PlacedAt] datetime2 NOT NULL,
        [Total] bigint NOT NULL,
        [CancelReason] nvarchar(300) NULL,
        [DeliveryNote] nvarchar(300) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ORDERS] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ORDERS_ADDRESS_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [ADDRESS] ([Id]),
        CONSTRAINT [FK_ORDERS_HUB_HubId] FOREIGN KEY ([HubId]) REFERENCES [HUB] ([Id]),
        CONSTRAINT [FK_ORDERS_USERS_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [USERS] ([Id]),
        CONSTRAINT [FK_ORDERS_USERS_ShipperId] FOREIGN KEY ([ShipperId]) REFERENCES [USERS] ([Id]),
        CONSTRAINT [FK_ORDERS_VOUCHER_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [VOUCHER] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [SYSTEM_SETTING] (
        [Key] varchar(40) NOT NULL,
        [Value] nvarchar(200) NOT NULL,
        [ChangedById] int NULL,
        [ChangedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SYSTEM_SETTING] PRIMARY KEY ([Key]),
        CONSTRAINT [FK_SYSTEM_SETTING_USERS_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [USERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [VERIFICATION_CODE] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Channel] varchar(5) NOT NULL,
        [CodeHash] nvarchar(100) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [Attempts] tinyint NOT NULL,
        [UsedAt] datetime2 NULL,
        CONSTRAINT [PK_VERIFICATION_CODE] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VERIFICATION_CODE_USERS_UserId] FOREIGN KEY ([UserId]) REFERENCES [USERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [ORDER_LINE] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [MenuItemId] int NOT NULL,
        [UnitPrice] bigint NOT NULL,
        [Quantity] int NOT NULL,
        CONSTRAINT [PK_ORDER_LINE] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ORDER_LINE_Quantity] CHECK ([Quantity] BETWEEN 1 AND 20),
        CONSTRAINT [FK_ORDER_LINE_MENU_ITEM_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MENU_ITEM] ([Id]),
        CONSTRAINT [FK_ORDER_LINE_ORDERS_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ORDERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [ORDER_STATUS_HISTORY] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [FromStatus] varchar(16) NULL,
        [ToStatus] varchar(16) NOT NULL,
        [ActorId] int NOT NULL,
        [At] datetime2 NOT NULL,
        [Reason] nvarchar(300) NULL,
        CONSTRAINT [PK_ORDER_STATUS_HISTORY] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ORDER_STATUS_HISTORY_ORDERS_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ORDERS] ([Id]),
        CONSTRAINT [FK_ORDER_STATUS_HISTORY_USERS_ActorId] FOREIGN KEY ([ActorId]) REFERENCES [USERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [PAYMENT] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [Method] varchar(10) NOT NULL,
        [Kind] varchar(7) NOT NULL,
        [Amount] bigint NOT NULL,
        [Status] varchar(9) NOT NULL,
        [GatewayRef] varchar(40) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PAYMENT] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PAYMENT_ORDERS_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ORDERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE TABLE [PRICE_SNAPSHOT] (
        [OrderId] int NOT NULL,
        [PlacedAt] datetime2 NOT NULL,
        [HubLoadPercent] decimal(5,2) NOT NULL,
        [HeavyRain] bit NOT NULL,
        [DistanceKm] decimal(5,2) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [QuietDiscount] decimal(18,4) NOT NULL,
        [VoucherDiscount] decimal(18,4) NOT NULL,
        [FoodTotal] decimal(18,4) NOT NULL,
        [DeliveryFee] decimal(18,4) NOT NULL,
        [RushHourSurcharge] decimal(18,4) NOT NULL,
        [RainSurcharge] decimal(18,4) NOT NULL,
        [RawTotal] decimal(18,4) NOT NULL,
        [Total] bigint NOT NULL,
        CONSTRAINT [PK_PRICE_SNAPSHOT] PRIMARY KEY ([OrderId]),
        CONSTRAINT [FK_PRICE_SNAPSHOT_ORDERS_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ORDERS] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ADDRESS_CustomerId] ON [ADDRESS] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AUDIT_LOG_ActorId] ON [AUDIT_LOG] ([ActorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AUDIT_LOG_At] ON [AUDIT_LOG] ([At]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BRAND_Name] ON [BRAND] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HUB_DeactivatedById] ON [HUB] ([DeactivatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_HUB_Name] ON [HUB] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HUB_BRAND_BrandId] ON [HUB_BRAND] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MENU_ITEM_BrandId] ON [MENU_ITEM] ([BrandId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDER_LINE_MenuItemId] ON [ORDER_LINE] ([MenuItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDER_LINE_OrderId] ON [ORDER_LINE] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDER_STATUS_HISTORY_ActorId] ON [ORDER_STATUS_HISTORY] ([ActorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDER_STATUS_HISTORY_OrderId_At] ON [ORDER_STATUS_HISTORY] ([OrderId], [At]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDERS_AddressId] ON [ORDERS] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ORDERS_Code] ON [ORDERS] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDERS_CustomerId_PlacedAt] ON [ORDERS] ([CustomerId], [PlacedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDERS_HubId_Status] ON [ORDERS] ([HubId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDERS_ShipperId] ON [ORDERS] ([ShipperId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ORDERS_VoucherId] ON [ORDERS] ([VoucherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PAYMENT_OrderId] ON [PAYMENT] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SYSTEM_SETTING_ChangedById] ON [SYSTEM_SETTING] ([ChangedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_USERS_Email] ON [USERS] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_USERS_HubId] ON [USERS] ([HubId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_USERS_PassportNumber_Nationality] ON [USERS] ([PassportNumber], [Nationality]) WHERE [PassportNumber] IS NOT NULL AND [Nationality] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_USERS_PhoneNumber] ON [USERS] ([PhoneNumber]) WHERE [PhoneNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_VERIFICATION_CODE_UserId] ON [VERIFICATION_CODE] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VOUCHER_Code] ON [VOUCHER] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    ALTER TABLE [ADDRESS] ADD CONSTRAINT [FK_ADDRESS_USERS_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [USERS] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    ALTER TABLE [AUDIT_LOG] ADD CONSTRAINT [FK_AUDIT_LOG_USERS_ActorId] FOREIGN KEY ([ActorId]) REFERENCES [USERS] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    ALTER TABLE [HUB] ADD CONSTRAINT [FK_HUB_USERS_DeactivatedById] FOREIGN KEY ([DeactivatedById]) REFERENCES [USERS] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261128014329_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261128014329_InitialCreate', N'10.0.15');
END;

COMMIT;
GO

