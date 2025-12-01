using DashboardMoto.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Motorbike> Motorbikes { get; set; }
	public DbSet<Brand> Brands { get; set; }
	public DbSet<Fuel> Fuels { get; set; }
	public DbSet<Gearbox> Gearboxes { get; set; }
	public DbSet<Seller> Sellers { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Motorbike>()
			.HasOne(m => m.Seller)
			.WithMany()
			.HasForeignKey(m => m.SellerId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}