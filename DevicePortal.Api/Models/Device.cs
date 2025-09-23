namespace DevicePortal.Api.Models;

public enum DeviceStatus
{
    Active = 1,
    Retired = 2,
    UnderRepair = 3
}

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Active;
}