using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public interface IMotoRepository
{
    public void Create(Motorbike motorbike);
    public IEnumerable<Motorbike> GetAll();
}
