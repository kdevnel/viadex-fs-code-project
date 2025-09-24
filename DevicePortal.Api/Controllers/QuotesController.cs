using DevicePortal.Api.DTOs;
using DevicePortal.Api.Extensions;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevicePortal.Api.Controllers;

[ApiController]
[Route( "api/[controller]" )]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController( IQuoteService quoteService )
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetQuotes( [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? supportTier = null )
    {
        if ( page < 1 || pageSize < 1 || pageSize > 100 )
        {
            return BadRequest( "Invalid pagination parameters" );
        }

        SupportTier? supportTierEnum = null;
        if ( supportTier.HasValue )
        {
            if ( !Enum.IsDefined( typeof( SupportTier ), supportTier.Value ) )
            {
                return BadRequest( "Invalid support tier" );
            }
            supportTierEnum = ( SupportTier ) supportTier.Value;
        }

        var result = await _quoteService.GetQuotesAsync( page, pageSize, supportTierEnum );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        var response = new {
            total = result.Total,
            items = result.Items.Select( q => q.ToDto() ).ToList(),
            isSuccess = true
        };

        return Ok( response );
    }

    [HttpGet( "{id}" )]
    public async Task<IActionResult> GetQuoteById( int id )
    {
        var quote = await _quoteService.GetQuoteByIdAsync( id );

        if ( quote == null )
        {
            return NotFound();
        }

        return Ok( quote.ToDto() );
    }

    [HttpPost( "calculate" )]
    public async Task<IActionResult> CalculateQuote( [FromBody] QuoteCalculateDto calculateDto )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        if ( !Enum.IsDefined( typeof( SupportTier ), calculateDto.SupportTier ) )
        {
            return BadRequest( "Invalid support tier" );
        }

        var result = await _quoteService.CalculateQuoteAsync(
            calculateDto.DeviceId,
            calculateDto.CustomerName,
            calculateDto.DurationMonths,
            ( SupportTier ) calculateDto.SupportTier
        );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        return Ok( result.Quote!.ToDto() );
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote( [FromBody] QuoteCreateDto createDto )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        if ( !Enum.IsDefined( typeof( SupportTier ), createDto.SupportTier ) )
        {
            return BadRequest( "Invalid support tier" );
        }

        var quote = createDto.ToEntity();
        var result = await _quoteService.CreateQuoteAsync( quote );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        return CreatedAtAction(
            nameof( GetQuoteById ),
            new { id = result.Quote!.Id },
            result.Quote.ToDto()
        );
    }

    [HttpGet( "support-tier-distribution" )]
    public async Task<IActionResult> GetSupportTierDistribution()
    {
        var result = await _quoteService.GetSupportTierDistributionAsync();

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        return Ok( result.Distribution.ToDistributionDto() );
    }
}