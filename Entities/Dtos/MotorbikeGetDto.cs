namespace DashboardMoto.Entities.Dtos;

public class MotorbikeGetDto
{
	public double? HorsePower { get; set; }
	public required string Model { get; set; }
	public DateTime PostDate { get; set; }
	public double Price { get; set; }
	public double MileageKm { get; set; }
	public required string Location { get; set; }

	public string? GearBoxType { get; set; }
	public required string BrandName { get; set; }
	public string? FuelType { get; set; }
	public required string SellerName { get; set; }
}
