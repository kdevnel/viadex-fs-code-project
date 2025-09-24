using DevicePortal.Api.DTOs;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.Extensions;

public static class QuoteMappingExtensions
{
    public static Quote ToEntity( this QuoteCreateDto dto )
    {
        return new Quote {
            DeviceId = dto.DeviceId,
            CustomerName = dto.CustomerName.Trim(),
            DurationMonths = dto.DurationMonths,
            SupportTier = ( SupportTier ) dto.SupportTier,
            ValidUntil = dto.ValidUntil ?? DateTime.UtcNow.AddDays( 30 )
        };
    }

    public static QuoteResponseDto ToDto( this Quote quote )
    {
        return new QuoteResponseDto {
            Id = quote.Id,
            DeviceId = quote.DeviceId,
            DeviceName = quote.Device?.Name ?? "",
            DeviceModel = quote.Device?.Model ?? "",
            CustomerName = quote.CustomerName,
            DurationMonths = quote.DurationMonths,
            SupportTier = ( int ) quote.SupportTier,
            SupportTierName = GetSupportTierName( quote.SupportTier ),
            MonthlyRate = quote.MonthlyRate,
            SupportRate = quote.SupportRate,
            TotalMonthlyCost = quote.TotalMonthlyCost,
            TotalCost = quote.TotalCost,
            CreatedAt = quote.CreatedAt,
            ValidUntil = quote.ValidUntil
        };
    }

    public static QuoteSupportTierDistributionDto ToDistributionDto( this Dictionary<string, int> distribution )
    {
        return new QuoteSupportTierDistributionDto {
            Basic = distribution.GetValueOrDefault( "Basic", 0 ),
            Standard = distribution.GetValueOrDefault( "Standard", 0 ),
            Premium = distribution.GetValueOrDefault( "Premium", 0 )
        };
    }

    private static string GetSupportTierName( SupportTier supportTier )
    {
        return supportTier switch {
            SupportTier.Basic => "Basic",
            SupportTier.Standard => "Standard",
            SupportTier.Premium => "Premium",
            _ => "Unknown"
        };
    }
}