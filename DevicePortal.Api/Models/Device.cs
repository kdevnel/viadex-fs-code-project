namespace DevicePortal.Api.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
}