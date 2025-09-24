using DevicePortal.Api.Models;

namespace DevicePortal.Api.Services.Interfaces;

public interface IQuoteService
{
    Task<QuotePagedResult> GetQuotesAsync( int page, int pageSize, SupportTier? supportTier = null );
    Task<Quote?> GetQuoteByIdAsync( int id );
    Task<QuoteCalculateResult> CalculateQuoteAsync( int deviceId, string customerName, int durationMonths, SupportTier supportTier );
    Task<QuoteCreateResult> CreateQuoteAsync( Quote quote );
    Task<QuoteSupportTierDistributionResult> GetSupportTierDistributionAsync();
}

public class QuotePagedResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int Total { get; init; }
    public List<Quote> Items { get; init; } = new();

    public static QuotePagedResult Success( int total, List<Quote> items ) =>
        new() { IsSuccess = true, Total = total, Items = items };

    public static QuotePagedResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class QuoteCalculateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Quote? Quote { get; init; }

    public static QuoteCalculateResult Success( Quote quote ) =>
        new() { IsSuccess = true, Quote = quote };

    public static QuoteCalculateResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class QuoteCreateResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Quote? Quote { get; init; }

    public static QuoteCreateResult Success( Quote quote ) =>
        new() { IsSuccess = true, Quote = quote };

    public static QuoteCreateResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

public class QuoteSupportTierDistributionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, int> Distribution { get; init; } = new();

    public static QuoteSupportTierDistributionResult Success( Dictionary<string, int> distribution ) =>
        new() { IsSuccess = true, Distribution = distribution };

    public static QuoteSupportTierDistributionResult Failure( string errorMessage ) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}