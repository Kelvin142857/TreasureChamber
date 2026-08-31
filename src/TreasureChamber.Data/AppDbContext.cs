using Microsoft.EntityFrameworkCore;
using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpec> ProductSpecs => Set<ProductSpec>();
    public DbSet<IntentOrder> IntentOrders => Set<IntentOrder>();
    public DbSet<IntentOrderItem> IntentOrderItems => Set<IntentOrderItem>();
    public DbSet<SystemSetting> Settings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Name);
            e.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Series>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Model).IsUnique();
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasOne(x => x.Series)
                .WithMany()
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasIndex(x => x.ProductId);
            e.HasOne(x => x.Product)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductSpec>(e =>
        {
            e.HasIndex(x => x.ProductId);
            e.HasOne(x => x.Product)
                .WithMany(x => x.Specs)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IntentOrder>(e =>
        {
            e.HasIndex(x => x.OrderNo).IsUnique();
            e.Property(x => x.OrderNo).HasMaxLength(30);
        });

        modelBuilder.Entity<IntentOrderItem>(e =>
        {
            e.HasIndex(x => x.IntentOrderId);
            e.HasOne(x => x.IntentOrder)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.IntentOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.HasKey(x => x.Key);
        });
    }
}
