using DevicePortal.Api.Models;

namespace DevicePortal.Api.Services.Interfaces;

public interface IShipmentService
{
    Task<ShipmentPagedResult> GetShipmentsAsync( int page, int pageSize, ShipmentStatus? status = null );
    Task<Shipment?> GetShipmentByIdAsync( int id );
    Task<Shipment?> GetShipmentByTrackingNumberAsync( string trackingNumber );
    Task<ShipmentCreateResult> CreateShipmentAsync( Shipment shipment );
    Task<ShipmentUpdateResult> UpdateShipmentStatusAsync( int id, ShipmentStatus status, DateTime? actualDelivery = null );
    Task<ShipmentStatusDistributionResult> GetShipmentStatusDistributionAsync();
}

public class ShipmentPagedResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int Total { get; init; }
    public List<Shipment> Items { get; init; } = new();

    public static ShipmentPagedResult Success( int total, List<Shipment> items ) =>
        new() { IsSuccess = true, Total = total, Items = items };

    public static ShipmentPagedResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class ShipmentCreateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Shipment? Shipment { get; init; }

    public static ShipmentCreateResult Success( Shipment shipment ) =>
        new() { IsSuccess = true, Shipment = shipment };

    public static ShipmentCreateResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class ShipmentUpdateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Shipment? Shipment { get; init; }

    public static ShipmentUpdateResult Success( Shipment shipment ) =>
        new() { IsSuccess = true, Shipment = shipment };

    public static ShipmentUpdateResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class ShipmentStatusDistributionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, int> Distribution { get; init; } = new();

    public static ShipmentStatusDistributionResult Success( Dictionary<string, int> distribution ) =>
        new() { IsSuccess = true, Distribution = distribution };

    public static ShipmentStatusDistributionResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}