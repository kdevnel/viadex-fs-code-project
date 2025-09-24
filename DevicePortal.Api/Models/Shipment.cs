namespace DevicePortal.Api.Models;

public enum ShipmentStatus
{
    Processing = 1,
    InTransit = 2,
    Delivered = 3,
    Delayed = 4
}

public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Processing;
    public DateTime EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Destination { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}