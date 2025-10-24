using DashboardMoto.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Motorbike> Motorbikes { get; set; }
    public DbSet<Seller> Sellers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Motorbike>()
            .HasOne(m => m.Seller)
            .WithMany()
            .HasForeignKey(m => m.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
