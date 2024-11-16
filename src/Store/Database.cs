using ECommerce.Types;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Store;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<CustomerCart> CustomerCarts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderTransaction> OrderTransactions { get; set; }
    public DbSet<CustomerOrderHistory> CustomerOrderHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerAddress>().
            HasOne<Customer>(ca => ca.Customer).
            WithMany(c => c.CustomerAddresses).
            HasForeignKey(ca => ca.CustomerId).
            OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerOrderHistory>().
            HasOne<Customer>(coh => coh.Customer).
            WithMany(c => c.CustomerOrderHistories).
            HasForeignKey(coh => coh.CustomerId).
            OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerCart>().
            HasOne<Product>(cc => cc.Product).
            WithMany(p => p.CustomerCarts).
            HasForeignKey(cc => cc.ProductId).
            OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerCart>().
            HasOne<Customer>(cc => cc.Customer).
            WithMany(c => c.CustomerCarts).
            HasForeignKey(cc => cc.CustomerId).
            OnDelete(DeleteBehavior.Cascade);

        //TODO: THINK AGAIN!
        modelBuilder.Entity<Product>().
            HasOne<ProductCategory>(pc => pc.ProductCategory).
            WithMany(c => c.Products).
            HasForeignKey(pc => pc.ProductCategoryId).
            OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Order>().
            HasOne<OrderTransaction>(o => o.Transaction).
            WithOne(ot => ot.Order).
            HasForeignKey<OrderTransaction>(ot => ot.OrderId).
            OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>().
            HasOne<Order>(oi => oi.Order).
            WithMany(o => o.OrderItems).
            HasForeignKey(oi => oi.OrderId).
            OnDelete(DeleteBehavior.Cascade);
    }

}
