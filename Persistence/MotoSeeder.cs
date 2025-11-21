using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public class MotoSeeder(AppDbContext dbContext)
{
	public async Task SeedAsync()
	{
		if (await dbContext.Database.CanConnectAsync())
		{
			if (!dbContext.Brands.Any())
			{
				var brands = GetPreconfiguredBrands();
				dbContext.Brands.AddRange(brands);
				await dbContext.SaveChangesAsync();
			}
			if (!dbContext.Fuels.Any())
			{
				var fuels = GetPreconfiguredFuels();
				dbContext.Fuels.AddRange(fuels);
				await dbContext.SaveChangesAsync();
			}
			if (!dbContext.Gearboxes.Any())
			{
				var gearboxes = GetPreconfiguredGearboxes();
				dbContext.Gearboxes.AddRange(gearboxes);
				await dbContext.SaveChangesAsync();
			}
		}
	}
	private static List<Brand> GetPreconfiguredBrands()
	{
		return
		[
			new() { Name = "Aprilia" },
			new() { Name = "BMW" },
			new() { Name = "Ducati" },
			new() { Name = "HarleyDavidson" },
			new() { Name = "Honda" },
			new() { Name = "Kawasaki" },
			new() { Name = "KTM" },
			new() { Name = "Suzuki" },
			new() { Name = "Triumph" },
			new() { Name = "Yamaha" },
			new() { Name = "MotoGuzzi" },
			new() { Name = "MvAgusta" },
			new() { Name = "Benelli" },
			new() { Name = "Piaggio" },
			new() { Name = "Indian" },
			new() { Name = "RoyalEnfield" },
			new() { Name = "Other" },
		];
	}
	private static List<Fuel> GetPreconfiguredFuels()
	{
		return
		[
			new() { Name = "Gasoline" },
			new() { Name = "Electric" },
			new() { Name = "Other" },
		];
	}
	private static List<Gearbox> GetPreconfiguredGearboxes()
	{
		return
		[
			new() { Name = "Manual" },
			new() { Name = "Automatic" },
			new() { Name = "SemiAutomatic" },
		];
	}
}