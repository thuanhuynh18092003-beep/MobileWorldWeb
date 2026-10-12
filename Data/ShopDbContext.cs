using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Data;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ParentCategory> ParentCategories => Set<ParentCategory>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<DiscountOffer> DiscountOffers => Set<DiscountOffer>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<VariantColor> VariantColors => Set<VariantColor>();
    public DbSet<VariantStorage> VariantStorages => Set<VariantStorage>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<User>().HasIndex(x => x.Email).IsUnique();
        model.Entity<Order>().HasIndex(x => x.Code).IsUnique();
        model.Entity<ParentCategory>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<Subcategory>().HasIndex(x => new { x.ParentCategoryId, x.Slug }).IsUnique();
        model.Entity<DiscountOffer>().HasIndex(x => x.Code).IsUnique();
        model.Entity<ProductVariant>().HasIndex(x => new { x.ProductId, x.ColorName, x.Storage }).IsUnique();
        model.Entity<VariantColor>().HasIndex(x => x.Name).IsUnique();
        model.Entity<VariantStorage>().HasIndex(x => x.Name).IsUnique();
        model.Entity<ProductVariant>().HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ProductVariant>().HasOne(x => x.VariantColor).WithMany(x => x.Variants).HasForeignKey(x => x.VariantColorId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<ProductVariant>().HasOne(x => x.VariantStorage).WithMany(x => x.Variants).HasForeignKey(x => x.VariantStorageId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Subcategory>().HasOne(x => x.ParentCategory).WithMany(x => x.Subcategories).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Product>().HasOne(x => x.Subcategory).WithMany(x => x.Products).HasForeignKey(x => x.SubcategoryId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Product>().Property(x => x.Price).HasPrecision(18, 0);
        model.Entity<Product>().Property(x => x.OldPrice).HasPrecision(18, 0);
        model.Entity<Order>().Property(x => x.Total).HasPrecision(18, 0);
        model.Entity<Order>().Property(x => x.Subtotal).HasPrecision(18, 0);
        model.Entity<Order>().Property(x => x.DiscountAmount).HasPrecision(18, 0);
        model.Entity<DiscountOffer>().Property(x => x.Value).HasPrecision(18, 0);
        model.Entity<DiscountOffer>().Property(x => x.MinimumOrder).HasPrecision(18, 0);
        model.Entity<DiscountOffer>().Property(x => x.MaximumDiscount).HasPrecision(18, 0);
        model.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 0);
        model.Entity<ProductVariant>().Property(x => x.Price).HasPrecision(18, 0);
        model.Entity<ProductVariant>().Property(x => x.OldPrice).HasPrecision(18, 0);
        model.Entity<OrderItem>().HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<OrderItem>().HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId);
    }
}
