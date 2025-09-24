using System.ComponentModel.DataAnnotations;

namespace DevicePortal.Api.DTOs;

public class ShipmentCreateDto
{
    [Required( ErrorMessage = "Tracking number is required" )]
    [StringLength( 50, ErrorMessage = "Tracking number cannot exceed 50 characters" )]
    public string TrackingNumber { get; set; } = "";

    [Required( ErrorMessage = "Customer name is required" )]
    [StringLength( 100, ErrorMessage = "Customer name cannot exceed 100 characters" )]
    public string CustomerName { get; set; } = "";

    [Required( ErrorMessage = "Estimated delivery date is required" )]
    public DateTime EstimatedDelivery { get; set; }

    [Required( ErrorMessage = "Destination is required" )]
    [StringLength( 200, ErrorMessage = "Destination cannot exceed 200 characters" )]
    public string Destination { get; set; } = "";
}

public class ShipmentResponseDto
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int Status { get; set; }
    public string StatusName { get; set; } = "";
    public DateTime EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Destination { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class ShipmentUpdateStatusDto
{
    [Range( 1, 4, ErrorMessage = "Status must be between 1 and 4" )]
    public int Status { get; set; }

    public DateTime? ActualDelivery { get; set; }
}

public class ShipmentStatusDistributionDto
{
    public int Processing { get; set; }
    public int InTransit { get; set; }
    public int Delivered { get; set; }
    public int Delayed { get; set; }
}