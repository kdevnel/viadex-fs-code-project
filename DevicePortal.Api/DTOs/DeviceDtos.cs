using System.ComponentModel.DataAnnotations;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.DTOs;

public class DeviceCreateDto
{
    [Required( ErrorMessage = "Name is required" )]
    [StringLength( 100, ErrorMessage = "Name cannot exceed 100 characters" )]
    public string Name { get; set; } = string.Empty;

    [Required( ErrorMessage = "Model is required" )]
    [StringLength( 50, ErrorMessage = "Model cannot exceed 50 characters" )]
    public string Model { get; set; } = string.Empty;

    [Required( ErrorMessage = "Monthly price is required" )]
    [Range( 0.01, 10000, ErrorMessage = "Monthly price must be between £0.01 and £10,000" )]
    public decimal MonthlyPrice { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Active;
}

public class DeviceResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DeviceStatus Status { get; set; }
}

public class DevicePagedResponseDto
{
    public int Total { get; set; }
    public List<DeviceResponseDto> Items { get; set; } = new();
}

public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}

public class DeviceStatusDistributionDto
{
    public Dictionary<string, int> Distribution { get; set; } = new();
}