using Microsoft.EntityFrameworkCore;

namespace ShopMobileWorld.Data;

// EnsureCreated does not update an existing SQL Server database. Keep these additions
// idempotent so older shops can start without losing products or orders.
public static class SchemaUpgrade
{
    public static async Task ApplyAsync(ShopDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("IF COL_LENGTH(N'dbo.Products', N'ImageUrl') IS NULL ALTER TABLE [dbo].[Products] ADD [ImageUrl] nvarchar(300) NULL");
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ParentCategories', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ParentCategories] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] nvarchar(80) NOT NULL, [Slug] nvarchar(100) NOT NULL, [DisplayOrder] int NOT NULL);
                CREATE UNIQUE INDEX [IX_ParentCategories_Slug] ON [dbo].[ParentCategories] ([Slug]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.Subcategories', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Subcategories] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [ParentCategoryId] int NOT NULL, [Name] nvarchar(80) NOT NULL, [Slug] nvarchar(100) NOT NULL, [DisplayOrder] int NOT NULL,
                    CONSTRAINT [FK_Subcategories_ParentCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [dbo].[ParentCategories] ([Id]));
                CREATE UNIQUE INDEX [IX_Subcategories_ParentCategoryId_Slug] ON [dbo].[Subcategories] ([ParentCategoryId], [Slug]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'dbo.Products', N'SubcategoryId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Products] ADD [SubcategoryId] int NULL;
                CREATE INDEX [IX_Products_SubcategoryId] ON [dbo].[Products] ([SubcategoryId]);
                ALTER TABLE [dbo].[Products] ADD CONSTRAINT [FK_Products_Subcategories_SubcategoryId] FOREIGN KEY ([SubcategoryId]) REFERENCES [dbo].[Subcategories] ([Id]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.DiscountOffers', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DiscountOffers] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [Code] nvarchar(30) NOT NULL, [Name] nvarchar(120) NOT NULL, [Type] nvarchar(10) NOT NULL,
                    [Value] decimal(18,0) NOT NULL, [MinimumOrder] decimal(18,0) NOT NULL, [MaximumDiscount] decimal(18,0) NULL,
                    [StartsAt] datetime2 NOT NULL, [EndsAt] datetime2 NOT NULL, [UsageLimit] int NULL, [UsedCount] int NOT NULL, [IsActive] bit NOT NULL);
                CREATE UNIQUE INDEX [IX_DiscountOffers_Code] ON [dbo].[DiscountOffers] ([Code]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'dbo.Orders', N'Subtotal') IS NULL ALTER TABLE [dbo].[Orders] ADD [Subtotal] decimal(18,0) NOT NULL CONSTRAINT [DF_Orders_Subtotal] DEFAULT (0);
            IF COL_LENGTH(N'dbo.Orders', N'DiscountAmount') IS NULL ALTER TABLE [dbo].[Orders] ADD [DiscountAmount] decimal(18,0) NOT NULL CONSTRAINT [DF_Orders_DiscountAmount] DEFAULT (0);
            IF COL_LENGTH(N'dbo.Orders', N'DiscountCode') IS NULL ALTER TABLE [dbo].[Orders] ADD [DiscountCode] nvarchar(30) NULL;
            IF COL_LENGTH(N'dbo.Orders', N'CompletedAt') IS NULL ALTER TABLE [dbo].[Orders] ADD [CompletedAt] datetime2 NULL;
            """);
        await db.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Orders] SET [Subtotal] = [Total] WHERE [Subtotal] = 0 AND [Total] > 0");
        await db.Database.ExecuteSqlRawAsync("UPDATE [dbo].[Orders] SET [CompletedAt] = [CreatedAt] WHERE [Status] = N'Hoàn thành' AND [CompletedAt] IS NULL");
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ProductVariants', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ProductVariants] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY, [ProductId] int NOT NULL,
                    [ColorName] nvarchar(60) NOT NULL, [ColorHex] nvarchar(7) NOT NULL, [Storage] nvarchar(30) NOT NULL,
                    [Price] decimal(18,0) NOT NULL, [OldPrice] decimal(18,0) NULL, [Stock] int NOT NULL,
                    [ImageUrl] nvarchar(300) NULL, [IsDefault] bit NOT NULL, [IsActive] bit NOT NULL,
                    CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]));
                CREATE UNIQUE INDEX [IX_ProductVariants_ProductId_ColorName_Storage] ON [dbo].[ProductVariants] ([ProductId], [ColorName], [Storage]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.VariantColors', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VariantColors] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Name] nvarchar(60) NOT NULL, [Hex] nvarchar(7) NOT NULL, [DisplayOrder] int NOT NULL);
                CREATE UNIQUE INDEX [IX_VariantColors_Name] ON [dbo].[VariantColors] ([Name]);
            END
            IF OBJECT_ID(N'dbo.VariantStorages', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VariantStorages] ([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Name] nvarchar(30) NOT NULL, [DisplayOrder] int NOT NULL);
                CREATE UNIQUE INDEX [IX_VariantStorages_Name] ON [dbo].[VariantStorages] ([Name]);
            END
            IF COL_LENGTH(N'dbo.ProductVariants', N'VariantColorId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[ProductVariants] ADD [VariantColorId] int NULL;
                CREATE INDEX [IX_ProductVariants_VariantColorId] ON [dbo].[ProductVariants] ([VariantColorId]);
                ALTER TABLE [dbo].[ProductVariants] ADD CONSTRAINT [FK_ProductVariants_VariantColors_VariantColorId]
                    FOREIGN KEY ([VariantColorId]) REFERENCES [dbo].[VariantColors] ([Id]);
            END
            IF COL_LENGTH(N'dbo.ProductVariants', N'VariantStorageId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[ProductVariants] ADD [VariantStorageId] int NULL;
                CREATE INDEX [IX_ProductVariants_VariantStorageId] ON [dbo].[ProductVariants] ([VariantStorageId]);
                ALTER TABLE [dbo].[ProductVariants] ADD CONSTRAINT [FK_ProductVariants_VariantStorages_VariantStorageId]
                    FOREIGN KEY ([VariantStorageId]) REFERENCES [dbo].[VariantStorages] ([Id]);
            END
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'dbo.OrderItems', N'ProductVariantId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[OrderItems] ADD [ProductVariantId] int NULL;
                CREATE INDEX [IX_OrderItems_ProductVariantId] ON [dbo].[OrderItems] ([ProductVariantId]);
                ALTER TABLE [dbo].[OrderItems] ADD CONSTRAINT [FK_OrderItems_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [dbo].[ProductVariants] ([Id]);
            END
            IF COL_LENGTH(N'dbo.OrderItems', N'VariantLabel') IS NULL ALTER TABLE [dbo].[OrderItems] ADD [VariantLabel] nvarchar(100) NULL;
            """);
    }
}
