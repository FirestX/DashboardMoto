namespace Models;

public class Motorbike
{
    public int Id { get; set; }
    public double? HorsePower { get; set; }
    public string? Model { get; set; }
    public DateTime PostDate { get; set; }
    public double Price { get; set; }
    public Gearbox Gearbox { get; set; }
    public double MileageKm { get; set; }
    public string? Location { get; set; }
    public Brand Brand { get; set; }
    public FuelType FuelType { get; set; }
    public int SellerId { get; set; }
    public Seller? Seller { get; set; }
}
