using DevicePortal.Api.Data;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevicePortal.Api.Services;

public class QuoteService : IQuoteService
{
    private readonly AppDbContext _context;

    public QuoteService( AppDbContext context )
    {
        _context = context;
    }

    public async Task<QuotePagedResult> GetQuotesAsync( int page, int pageSize, SupportTier? supportTier = null )
    {
        try
        {
            var query = _context.Quotes
                .Include( q => q.Device )
                .AsQueryable();

            if ( supportTier.HasValue )
            {
                query = query.Where( q => q.SupportTier == supportTier.Value );
            }

            var total = await query.CountAsync();

            var quotes = await query
                .OrderByDescending( q => q.CreatedAt )
                .Skip( (page - 1) * pageSize )
                .Take( pageSize )
                .ToListAsync();

            return QuotePagedResult.Success( total, quotes );
        }
        catch ( Exception ex )
        {
            return QuotePagedResult.Failure( $"Failed to retrieve quotes: {ex.Message}" );
        }
    }

    public async Task<Quote?> GetQuoteByIdAsync( int id )
    {
        try
        {
            return await _context.Quotes
                .Include( q => q.Device )
                .FirstOrDefaultAsync( q => q.Id == id );
        }
        catch
        {
            return null;
        }
    }

    public async Task<QuoteCalculateResult> CalculateQuoteAsync( int deviceId, string customerName, int durationMonths, SupportTier supportTier )
    {
        try
        {
            // Validate device exists and is active
            var device = await _context.Devices.FindAsync( deviceId );
            if ( device == null )
            {
                return QuoteCalculateResult.Failure( "Device not found" );
            }

            if ( device.Status != DeviceStatus.Active )
            {
                return QuoteCalculateResult.Failure( "Device is not available for leasing" );
            }

            // Calculate pricing based on support tier
            var monthlyRate = device.MonthlyPrice;
            var supportMultiplier = GetSupportTierMultiplier( supportTier );
            var supportRate = monthlyRate * supportMultiplier;
            var totalMonthlyCost = monthlyRate + supportRate;
            var totalCost = totalMonthlyCost * durationMonths;

            // Create quote (not saved to database yet)
            var quote = new Quote {
                DeviceId = deviceId,
                Device = device,
                CustomerName = customerName.Trim(),
                DurationMonths = durationMonths,
                SupportTier = supportTier,
                MonthlyRate = monthlyRate,
                SupportRate = supportRate,
                TotalMonthlyCost = totalMonthlyCost,
                TotalCost = totalCost,
                CreatedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays( 30 ) // Quote valid for 30 days
            };

            return QuoteCalculateResult.Success( quote );
        }
        catch ( Exception ex )
        {
            return QuoteCalculateResult.Failure( $"Failed to calculate quote: {ex.Message}" );
        }
    }

    public async Task<QuoteCreateResult> CreateQuoteAsync( Quote quote )
    {
        try
        {
            // Validate device exists and is active
            var device = await _context.Devices.FindAsync( quote.DeviceId );
            if ( device == null )
            {
                return QuoteCreateResult.Failure( "Device not found" );
            }

            if ( device.Status != DeviceStatus.Active )
            {
                return QuoteCreateResult.Failure( "Device is not available for leasing" );
            }

            // Recalculate to ensure accuracy
            var calculationResult = await CalculateQuoteAsync(
                quote.DeviceId,
                quote.CustomerName,
                quote.DurationMonths,
                quote.SupportTier
            );

            if ( !calculationResult.IsSuccess || calculationResult.Quote == null )
            {
                return QuoteCreateResult.Failure( calculationResult.ErrorMessage ?? "Failed to calculate quote" );
            }

            // Use calculated values
            quote.MonthlyRate = calculationResult.Quote.MonthlyRate;
            quote.SupportRate = calculationResult.Quote.SupportRate;
            quote.TotalMonthlyCost = calculationResult.Quote.TotalMonthlyCost;
            quote.TotalCost = calculationResult.Quote.TotalCost;
            quote.CreatedAt = DateTime.UtcNow;

            // Set default ValidUntil if not provided
            if ( quote.ValidUntil == default )
            {
                quote.ValidUntil = DateTime.UtcNow.AddDays( 30 );
            }

            _context.Quotes.Add( quote );
            await _context.SaveChangesAsync();

            // Return quote with device information
            var savedQuote = await GetQuoteByIdAsync( quote.Id );
            return QuoteCreateResult.Success( savedQuote! );
        }
        catch ( Exception ex )
        {
            return QuoteCreateResult.Failure( $"Failed to create quote: {ex.Message}" );
        }
    }

    public async Task<QuoteSupportTierDistributionResult> GetSupportTierDistributionAsync()
    {
        try
        {
            var distribution = await _context.Quotes
                .GroupBy( q => q.SupportTier )
                .Select( g => new { SupportTier = g.Key, Count = g.Count() } )
                .ToListAsync();

            var result = new Dictionary<string, int> {
                ["Basic"] = distribution.FirstOrDefault( d => d.SupportTier == SupportTier.Basic )?.Count ?? 0,
                ["Standard"] = distribution.FirstOrDefault( d => d.SupportTier == SupportTier.Standard )?.Count ?? 0,
                ["Premium"] = distribution.FirstOrDefault( d => d.SupportTier == SupportTier.Premium )?.Count ?? 0
            };

            return QuoteSupportTierDistributionResult.Success( result );
        }
        catch ( Exception ex )
        {
            return QuoteSupportTierDistributionResult.Failure( $"Failed to get support tier distribution: {ex.Message}" );
        }
    }

    private static decimal GetSupportTierMultiplier( SupportTier supportTier )
    {
        return supportTier switch {
            SupportTier.Basic => 0.0m,      // +0%
            SupportTier.Standard => 0.2m,   // +20%
            SupportTier.Premium => 0.5m,    // +50%
            _ => 0.0m
        };
    }
}