using DashboardMoto.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Persistence;

public class MotoRepository(AppDbContext dbContext) : IMotoRepository
{
	public async Task Create(Motorbike motorbike)
	{
		dbContext.Motorbikes.Add(motorbike);
		await dbContext.SaveChangesAsync();
	}

	public async Task CreateMany(List<Motorbike> motorbikes)
	{
		await dbContext.Motorbikes.AddRangeAsync(motorbikes);
		await dbContext.SaveChangesAsync();
	}
	public async Task<List<Motorbike>> GetAll()
	{
		return await dbContext.Motorbikes.ToListAsync();
	}
}