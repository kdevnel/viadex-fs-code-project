using DevicePortal.Api.DTOs;
using DevicePortal.Api.Extensions;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevicePortal.Api.Controllers;

[ApiController]
[Route( "api/[controller]" )]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController( IDeviceService deviceService )
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Get paginated list of devices
    /// </summary>
    /// <param name="page">Page number (starting from 1)</param>
    /// <param name="pageSize">Number of items per page (max 100)</param>
    /// <returns>Paginated device list</returns>
    [HttpGet]
    [ProducesResponseType( typeof( DevicePagedResponseDto ), StatusCodes.Status200OK )]
    [ProducesResponseType( typeof( ApiErrorResponse ), StatusCodes.Status400BadRequest )]
    public async Task<IActionResult> Get( [FromQuery] int page = 1, [FromQuery] int pageSize = 20 )
    {
        // ✅ Controller only handles HTTP concerns - delegate business logic to service
        var result = await _deviceService.GetDevicesAsync( page, pageSize );

        if ( !result.IsSuccess )
            return BadRequest( new ApiErrorResponse { Error = result.ErrorMessage! } );

        return Ok( result.ToPagedDto() );
    }

    /// <summary>
    /// Get device by ID
    /// </summary>
    /// <param name="id">Device ID</param>
    /// <returns>Device details</returns>
    [HttpGet( "{id:int}" )]
    [ProducesResponseType( typeof( DeviceResponseDto ), StatusCodes.Status200OK )]
    [ProducesResponseType( StatusCodes.Status404NotFound )]
    public async Task<IActionResult> GetById( int id )
    {
        // ✅ Controller only handles HTTP concerns - delegate to service
        var device = await _deviceService.GetDeviceByIdAsync( id );

        return device is null
            ? NotFound()
            : Ok( device.ToDto() );
    }

    /// <summary>
    /// Create a new device
    /// </summary>
    /// <param name="deviceDto">Device creation data</param>
    /// <returns>Created device</returns>
    [HttpPost]
    [ProducesResponseType( typeof( DeviceResponseDto ), StatusCodes.Status201Created )]
    [ProducesResponseType( typeof( ApiErrorResponse ), StatusCodes.Status400BadRequest )]
    public async Task<IActionResult> Post( [FromBody] DeviceCreateDto deviceDto )
    {
        // ✅ HTTP concern: Model validation
        if ( !ModelState.IsValid )
        {
            var errors = ModelState
                .SelectMany( x => x.Value!.Errors )
                .Select( x => x.ErrorMessage )
                .ToList();

            return BadRequest( new ApiErrorResponse {
                Error = "Validation failed",
                Details = errors
            } );
        }

        // ✅ HTTP concern: Convert DTO to entity and delegate to service
        var device = deviceDto.ToEntity();
        var result = await _deviceService.CreateDeviceAsync( device );

        if ( !result.IsSuccess )
            return BadRequest( new ApiErrorResponse { Error = result.ErrorMessage! } );

        var responseDto = result.Device!.ToDto();
        return CreatedAtAction( nameof( GetById ), new { id = responseDto.Id }, responseDto );
    }
}