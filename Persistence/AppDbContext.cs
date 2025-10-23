using DashboardMoto.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Motorbike> Motorbikes { get; set; }
    public DbSet<Seller> Sellers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseNpgsql(
            "Host=ep-muddy-dust-ab9o591a-pooler.eu-west-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_O3kHAnQoj5gC;"
        );

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Motorbike>()
            .HasOne(m => m.Seller)
            .WithMany()
            .HasForeignKey(m => m.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
