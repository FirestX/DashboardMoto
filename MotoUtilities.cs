using DashboardMoto.Entities;
using DashboardMoto.Entities.Dtos;
using DashboardMoto.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DashboardMoto;

public class MotoUtilities (IMotoRepository motoRepository, AppDbContext dbContext)
{
    public async Task PrintInDatabase(List<MotorbikeDto> motorbikesDto)
    {
        const int batchSize = 100;
        
        var gearboxes = await dbContext.Gearboxes.ToDictionaryAsync(g => g.Name, g => g.Id);
        var fuels = await dbContext.Fuels.ToDictionaryAsync(f => f.Name, f => f.Id);
        var brands = await dbContext.Brands.ToDictionaryAsync(b => b.Name, b => b);
        var sellers = await dbContext.Sellers.ToDictionaryAsync(s => s.Name, s => s);
        
        var motorbikes = new List<Motorbike>();
        foreach (var motorbike in motorbikesDto)
        {
            if (!sellers.TryGetValue(motorbike.SellerName, out var seller))
            {
                seller = new Seller
                {
                    Name = motorbike.SellerName,
                    Email = $"{motorbike.SellerName.Replace(" ", "").ToLower()}@example.com"
                };
                dbContext.Sellers.Add(seller);
                await dbContext.SaveChangesAsync();
                sellers[motorbike.SellerName] = seller;
            }
            
            motorbikes.Add(new Motorbike
            {
                Model = motorbike.Model,
                Price = motorbike.Price,
                MileageKm = motorbike.MileageKm,
                FuelId = motorbike.FuelType.HasValue ? fuels.GetValueOrDefault(motorbike.FuelType.Value.ToString()) : null,
                GearBoxId = motorbike.GearBoxType.HasValue ? gearboxes.GetValueOrDefault(motorbike.GearBoxType.Value.ToString()) : null,
                BrandId = brands[motorbike.BrandName.ToString()].Id,
                Brand = brands[motorbike.BrandName.ToString()],
                SellerId = seller.Id,
                Seller = seller,
                HorsePower = motorbike.HorsePower,
                Location = motorbike.Location,
                PostDate = motorbike.PostDate.ToUniversalTime()
            });
        }
        var batches = motorbikes
            .Select((m, i) => new { Motorbike = m, Index = i })
            .GroupBy(x => x.Index / batchSize)
            .Select(g => g.Select(x => x.Motorbike).ToList())
            .ToList();

        foreach (var batch in batches)
        {
            await motoRepository.CreateMany(batch);
        }
    }
}
