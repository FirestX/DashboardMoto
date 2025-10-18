namespace DashboardMoto.Entities;

class Motorbike(string name, string url, string price)
{
  public string? Url { get; set; } = url;
  public string? Name { get; set; } = name;
  public string? Price { get; set; } = price;
}
