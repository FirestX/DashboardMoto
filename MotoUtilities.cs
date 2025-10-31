using DashboardMoto.Entities;
using DashboardMoto.Persistence;

namespace DashboardMoto;

public class MotoUtilities (IMotoRepository motoRepository)
{
    public async Task PrintInDatabase(List<Motorbike> motorbikes)
    {
        foreach (var motorbike in motorbikes)
        {
            motorbike.SellerId = 1;
            motorbike.PostDate = motorbike.PostDate.ToUniversalTime();
            await motoRepository.Create(motorbike);
        }  
    }
}