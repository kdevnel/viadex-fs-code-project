using System.ComponentModel.DataAnnotations;

namespace DevicePortal.Api.DTOs;

public class QuoteCalculateDto
{
    [Required( ErrorMessage = "Device ID is required" )]
    [Range( 1, int.MaxValue, ErrorMessage = "Device ID must be a positive number" )]
    public int DeviceId { get; set; }

    [Required( ErrorMessage = "Customer name is required" )]
    [StringLength( 100, ErrorMessage = "Customer name cannot exceed 100 characters" )]
    public string CustomerName { get; set; } = "";

    [Required( ErrorMessage = "Duration is required" )]
    [Range( 1, 60, ErrorMessage = "Duration must be between 1 and 60 months" )]
    public int DurationMonths { get; set; }

    [Required( ErrorMessage = "Support tier is required" )]
    [Range( 1, 3, ErrorMessage = "Support tier must be between 1 and 3" )]
    public int SupportTier { get; set; }
}

public class QuoteResponseDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = "";
    public string DeviceModel { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int DurationMonths { get; set; }
    public int SupportTier { get; set; }
    public string SupportTierName { get; set; } = "";
    public decimal MonthlyRate { get; set; }
    public decimal SupportRate { get; set; }
    public decimal TotalMonthlyCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class QuoteCreateDto
{
    [Required( ErrorMessage = "Device ID is required" )]
    [Range( 1, int.MaxValue, ErrorMessage = "Device ID must be a positive number" )]
    public int DeviceId { get; set; }

    [Required( ErrorMessage = "Customer name is required" )]
    [StringLength( 100, ErrorMessage = "Customer name cannot exceed 100 characters" )]
    public string CustomerName { get; set; } = "";

    [Required( ErrorMessage = "Duration is required" )]
    [Range( 1, 60, ErrorMessage = "Duration must be between 1 and 60 months" )]
    public int DurationMonths { get; set; }

    [Required( ErrorMessage = "Support tier is required" )]
    [Range( 1, 3, ErrorMessage = "Support tier must be between 1 and 3" )]
    public int SupportTier { get; set; }

    public DateTime? ValidUntil { get; set; }
}

public class QuoteSupportTierDistributionDto
{
    public int Basic { get; set; }
    public int Standard { get; set; }
    public int Premium { get; set; }
}