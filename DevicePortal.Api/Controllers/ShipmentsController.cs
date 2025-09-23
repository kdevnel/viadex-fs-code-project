using DevicePortal.Api.DTOs;
using DevicePortal.Api.Extensions;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevicePortal.Api.Controllers;

[ApiController]
[Route( "api/[controller]" )]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController( IShipmentService shipmentService )
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ShipmentPagedResult>> GetShipments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ShipmentStatus? status = null )
    {
        var result = await _shipmentService.GetShipmentsAsync( page, pageSize, status );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        // Convert entities to DTOs
        var shipmentDtos = result.Items.Select( s => s.ToDto() ).ToList();

        return Ok( new {
            result.IsSuccess,
            result.Total,
            Items = shipmentDtos
        } );
    }

    [HttpGet( "{id}" )]
    public async Task<ActionResult<ShipmentResponseDto>> GetShipment( int id )
    {
        var shipment = await _shipmentService.GetShipmentByIdAsync( id );

        if ( shipment == null )
        {
            return NotFound( $"Shipment with ID {id} not found." );
        }

        return Ok( shipment.ToDto() );
    }

    [HttpGet( "track/{trackingNumber}" )]
    public async Task<ActionResult<ShipmentResponseDto>> TrackShipment( string trackingNumber )
    {
        if ( string.IsNullOrWhiteSpace( trackingNumber ) )
        {
            return BadRequest( "Tracking number is required." );
        }

        var shipment = await _shipmentService.GetShipmentByTrackingNumberAsync( trackingNumber );

        if ( shipment == null )
        {
            return NotFound( $"Shipment with tracking number '{trackingNumber}' not found." );
        }

        return Ok( shipment.ToDto() );
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentResponseDto>> CreateShipment( ShipmentCreateDto createDto )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var shipment = createDto.ToEntity();
        var result = await _shipmentService.CreateShipmentAsync( shipment );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        var responseDto = result.Shipment!.ToDto();
        return CreatedAtAction( nameof( GetShipment ), new { id = result.Shipment.Id }, responseDto );
    }

    [HttpPatch( "{id}/status" )]
    public async Task<ActionResult<ShipmentResponseDto>> UpdateShipmentStatus( int id, ShipmentUpdateStatusDto updateDto )
    {
        if ( !ModelState.IsValid )
        {
            return BadRequest( ModelState );
        }

        var status = ( ShipmentStatus ) updateDto.Status;
        var result = await _shipmentService.UpdateShipmentStatusAsync( id, status, updateDto.ActualDelivery );

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        return Ok( result.Shipment!.ToDto() );
    }

    [HttpGet( "status-distribution" )]
    public async Task<ActionResult<ShipmentStatusDistributionDto>> GetStatusDistribution()
    {
        var result = await _shipmentService.GetShipmentStatusDistributionAsync();

        if ( !result.IsSuccess )
        {
            return BadRequest( result.ErrorMessage );
        }

        var distribution = new ShipmentStatusDistributionDto {
            Processing = result.Distribution.GetValueOrDefault( "Processing", 0 ),
            InTransit = result.Distribution.GetValueOrDefault( "InTransit", 0 ),
            Delivered = result.Distribution.GetValueOrDefault( "Delivered", 0 ),
            Delayed = result.Distribution.GetValueOrDefault( "Delayed", 0 )
        };

        return Ok( distribution );
    }
}