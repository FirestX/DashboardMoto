using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public class MotoRepository(AppDbContext dbContext) : IMotoRepository
{
  public void Create(Motorbike motorbike)
  {
    throw new NotImplementedException();
  }
  public IEnumerable<Motorbike> GetAll()
  {
    throw new NotImplementedException();
  }
}
