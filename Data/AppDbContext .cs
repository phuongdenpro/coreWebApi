using coreWebApi.Models;
using DemoApi.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<UserProduct>()
    //        .HasKey(up => new { up.UserId, up.ProductId });

    //    modelBuilder.Entity<UserProduct>()
    //        .HasOne(up => up.User)
    //        .WithMany(u => u.UserProducts)
    //        .HasForeignKey(up => up.UserId);

    //    modelBuilder.Entity<UserProduct>()
    //        .HasOne(up => up.Product)
    //        .WithMany(p => p.UserProducts)
    //        .HasForeignKey(up => up.ProductId);
    //}
}