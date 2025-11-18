using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public interface IMotoRepository
{
    public Task Create(Motorbike motorbike);
    public Task CreateMany(List<Motorbike> motorbikes);
    public Task<List<Motorbike>> GetAll();
}
