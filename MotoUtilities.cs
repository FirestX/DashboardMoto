using DashboardMoto.Entities;
using DashboardMoto.Persistence;

namespace DashboardMoto;

public class MotoUtilities (IMotoRepository motoRepository)
{
    public async Task PrintInDatabase(List<Motorbike> motorbikes)
    {
        const int batchSize = 100;
        var batches = motorbikes
            .Select((m, i) => new { Motorbike = m, Index = i })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Motorbike).ToList())
            .ToList();

        var tasks = batches.Select(async batch =>
        {
            foreach (var motorbike in batch)
            {
                motorbike.SellerId = 1;
                motorbike.PostDate = motorbike.PostDate.ToUniversalTime();
            }
            await motoRepository.CreateMany(batch);
        });

        await Task.WhenAll(tasks);
    }
}