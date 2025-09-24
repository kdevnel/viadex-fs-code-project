using DevicePortal.Api.Models;

namespace DevicePortal.Api.Services.Interfaces;

public interface IDeviceService
{
    Task<DevicePagedResult> GetDevicesAsync( int page, int pageSize, DeviceStatus? status = null );
    Task<Device?> GetDeviceByIdAsync( int id );
    Task<DeviceCreateResult> CreateDeviceAsync( Device device );
    Task<DeviceDeleteResult> DeleteDeviceAsync( int id );
    Task<DeviceStatusDistributionResult> GetDeviceStatusDistributionAsync();
}

public class DevicePagedResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int Total { get; init; }
    public List<Device> Items { get; init; } = new();

    public static DevicePagedResult Success( int total, List<Device> items ) =>
        new() { IsSuccess = true, Total = total, Items = items };

    public static DevicePagedResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class DeviceCreateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Device? Device { get; init; }

    public static DeviceCreateResult Success( Device device ) =>
        new() { IsSuccess = true, Device = device };

    public static DeviceCreateResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class DeviceDeleteResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static DeviceDeleteResult Success() =>
        new() { IsSuccess = true };

    public static DeviceDeleteResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class DeviceStatusDistributionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<DeviceStatus, int> Distribution { get; init; } = new();

    public static DeviceStatusDistributionResult Success( Dictionary<DeviceStatus, int> distribution ) =>
        new() { IsSuccess = true, Distribution = distribution };

    public static DeviceStatusDistributionResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
