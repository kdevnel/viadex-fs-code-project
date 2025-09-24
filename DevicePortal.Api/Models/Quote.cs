namespace DevicePortal.Api.Models;

public enum SupportTier
{
    Basic = 1,      // +0% of monthly price
    Standard = 2,   // +20% of monthly price
    Premium = 3     // +50% of monthly price
}

public class Quote
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public string CustomerName { get; set; } = "";
    public int DurationMonths { get; set; }
    public SupportTier SupportTier { get; set; } = SupportTier.Basic;
    public decimal MonthlyRate { get; set; }
    public decimal SupportRate { get; set; }
    public decimal TotalMonthlyCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
}