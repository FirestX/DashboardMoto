using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Gearbox
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Motorbike> Motorbikes { get; set; } = [];
}
