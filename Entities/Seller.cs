using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Entities;

[Index(nameof(Email), IsUnique = true)]
public class Seller
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}
