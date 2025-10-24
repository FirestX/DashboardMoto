namespace DashboardMoto.Entities;

public class Motorbike(
    int id,
    double? horsePower,
    string model,
    DateTime postDate,
    double price,
    double mileageKm,
    string location,
    int sellerId,
    Brand brand,
    FuelType? fuelType = null,
    GearBox? gearBox = null
)
{
    public int Id { get; set; } = id;
    public double? HorsePower { get; set; } = horsePower;
    public string Model { get; set; } = model;
    public DateTime PostDate { get; set; } = postDate;
    public double Price { get; set; } = price;
    public GearBox? GearBox { get; set; } = gearBox;
    public double MileageKm { get; set; } = mileageKm;
    public string Location { get; set; } = location;
    public Brand Brand { get; set; } = brand;
    public FuelType? FuelType { get; set; } = fuelType;
    public int SellerId { get; set; } = sellerId;
    public Seller Seller { get; set; } = null!;
}
