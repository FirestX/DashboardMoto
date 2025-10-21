namespace DashboardMoto.Entities;

class Motorbike(int id, double horsePower, string model, DateTime postDate, double price, Enum gearBox, double mileageKm, string location, Enum brand, Enum fuelType, int sellerId)
{
  public int Id { get; set; } = id;
  public double? HorsePower { get; set; } = horsePower;
  public string Model { get; set; } = model;
  public DateTime? PostDate { get; set; } = postDate;
  public double Price { get; set; } = price;
  public Enum? GearBox { get; set; } = gearBox;
  public double MileageKm { get; set; } = mileageKm;
  public string Location { get; set; } = location;
  public Enum Brand { get; set; } = brand;
  public Enum? FuelType { get; set; } = fuelType;
  public int SellerId { get; set; } = sellerId;
}
