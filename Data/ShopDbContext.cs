using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Data;

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        model.Entity<User>().HasIndex(x => x.Email).IsUnique();
        model.Entity<Order>().HasIndex(x => x.Code).IsUnique();
        model.Entity<Product>().Property(x => x.Price).HasPrecision(18, 0);
        model.Entity<Product>().Property(x => x.OldPrice).HasPrecision(18, 0);
        model.Entity<Order>().Property(x => x.Total).HasPrecision(18, 0);
        model.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 0);
        model.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<OrderItem>().HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId);
    }
}
