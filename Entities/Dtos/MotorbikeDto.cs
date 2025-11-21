namespace DashboardMoto.Entities.Dtos;

public class MotorbikeDto(double? horsePower, string model, DateTime postDate, double price, double mileageKm, string location, string? gearBoxType, string brandName, string? fuelType, string sellerName)
{
	public double? HorsePower { get; set; } = horsePower;
	public required string Model { get; set; } = model;
	public required DateTime PostDate { get; set; } = postDate;
	public required double Price { get; set; } = price;
	public required double MileageKm { get; set; } = mileageKm;
	public required string Location { get; set; } = location;

	public string? GearBoxType { get; set; } = gearBoxType;
	public required string BrandName { get; set; } = brandName;
	public string? FuelType { get; set; } = fuelType;
	public required string SellerName { get; set; } = sellerName;
}