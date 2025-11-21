namespace DashboardMoto.Entities;

public class Motorbike
{
    public int Id { get; set; }
    public double? HorsePower { get; set; }
    public required string Model { get; set; }
    public required DateTime PostDate { get; set; }
    public required double Price { get; set; }
    public required double MileageKm { get; set; }
    public required string Location { get; set; }

    public int? GearBoxId { get; set; }
    public required int BrandId { get; set; }
    public int? FuelId { get; set; }
    public required int SellerId { get; set; }

    public Gearbox? Gearbox { get; set; }
    public required Brand Brand { get; set; }
    public Fuel? Fuel { get; set; }
    public required Seller Seller { get; set; }
}
