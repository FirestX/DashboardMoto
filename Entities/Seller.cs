using Microsoft.EntityFrameworkCore;

namespace DashboardMoto.Entities;

[Index(nameof(Email), IsUnique = true)]
public class Seller(int id, string name, string email)
{
    public int Id { get; set; } = id;
    public required string Name { get; set; } = name;
    public required string Email { get; set; } = email;
}
