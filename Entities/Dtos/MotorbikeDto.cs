namespace DashboardMoto.Entities.Dtos;

public class MotorbikeDto
{
	public double? HorsePower { get; set; }
	public required string Model { get; set; }
	public DateTime PostDate { get; set; }
	public double Price { get; set; }
	public double MileageKm { get; set; }
	public required string Location { get; set; }

	public GearboxDto? GearBoxType { get; set; }
	public BrandDto BrandName { get; set; }
	public FuelDto? FuelType { get; set; }
	public required string SellerName { get; set; }
}