using DashboardMoto.Entities;

namespace DashboardMoto.Persistence;

public interface IMotoRepository
{
    public Task Create(Motorbike motorbike);
    public Task<List<Motorbike>> GetAll();
}
