using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public class MotoRepository(AppDbContext dbContext) : IMotoRepository
{
  public void Create(Motorbike motorbike)
  {
    dbContext.Motorbikes.Add(motorbike);
    dbContext.SaveChanges();
  }
  public IEnumerable<Motorbike> GetAll()
  {
    return dbContext.Motorbikes.AsEnumerable();
  }
}
