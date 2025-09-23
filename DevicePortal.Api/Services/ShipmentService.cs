using DevicePortal.Api.Data;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevicePortal.Api.Services;

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _context;

    public ShipmentService( AppDbContext context )
    {
        _context = context;
    }

    public async Task<ShipmentPagedResult> GetShipmentsAsync( int page, int pageSize, ShipmentStatus? status = null )
    {
        try
        {
            if ( page < 1 ) page = 1;
            if ( pageSize < 1 || pageSize > 100 ) pageSize = 20;

            var query = _context.Shipments.AsQueryable();

            if ( status.HasValue )
            {
                query = query.Where( s => s.Status == status.Value );
            }

            var total = await query.CountAsync();
            var shipments = await query
                .OrderByDescending( s => s.CreatedAt )
                .Skip( (page - 1) * pageSize )
                .Take( pageSize )
                .ToListAsync();

            return ShipmentPagedResult.Success( total, shipments );
        }
        catch ( Exception ex )
        {
            return ShipmentPagedResult.Failure( $"Error retrieving shipments: {ex.Message}" );
        }
    }

    public async Task<Shipment?> GetShipmentByIdAsync( int id )
    {
        try
        {
            return await _context.Shipments.FindAsync( id );
        }
        catch
        {
            return null;
        }
    }

    public async Task<Shipment?> GetShipmentByTrackingNumberAsync( string trackingNumber )
    {
        try
        {
            return await _context.Shipments
                .FirstOrDefaultAsync( s => s.TrackingNumber == trackingNumber );
        }
        catch
        {
            return null;
        }
    }

    public async Task<ShipmentCreateResult> CreateShipmentAsync( Shipment shipment )
    {
        try
        {
            // Check if tracking number already exists
            var existingShipment = await _context.Shipments
                .FirstOrDefaultAsync( s => s.TrackingNumber == shipment.TrackingNumber );

            if ( existingShipment != null )
            {
                return ShipmentCreateResult.Failure( "A shipment with this tracking number already exists." );
            }

            // Validate estimated delivery date
            if ( shipment.EstimatedDelivery <= DateTime.UtcNow )
            {
                return ShipmentCreateResult.Failure( "Estimated delivery date must be in the future." );
            }

            _context.Shipments.Add( shipment );
            await _context.SaveChangesAsync();

            return ShipmentCreateResult.Success( shipment );
        }
        catch ( Exception ex )
        {
            return ShipmentCreateResult.Failure( $"Error creating shipment: {ex.Message}" );
        }
    }

    public async Task<ShipmentUpdateResult> UpdateShipmentStatusAsync( int id, ShipmentStatus status, DateTime? actualDelivery = null )
    {
        try
        {
            var shipment = await _context.Shipments.FindAsync( id );
            if ( shipment == null )
            {
                return ShipmentUpdateResult.Failure( "Shipment not found." );
            }

            // Business rules for status transitions
            if ( shipment.Status == ShipmentStatus.Delivered )
            {
                return ShipmentUpdateResult.Failure( "Cannot change status of a delivered shipment." );
            }

            // If setting to delivered, require actual delivery date
            if ( status == ShipmentStatus.Delivered && !actualDelivery.HasValue )
            {
                actualDelivery = DateTime.UtcNow;
            }

            shipment.Status = status;
            shipment.ActualDelivery = actualDelivery;

            await _context.SaveChangesAsync();
            return ShipmentUpdateResult.Success( shipment );
        }
        catch ( Exception ex )
        {
            return ShipmentUpdateResult.Failure( $"Error updating shipment: {ex.Message}" );
        }
    }

    public async Task<ShipmentStatusDistributionResult> GetShipmentStatusDistributionAsync()
    {
        try
        {
            var shipments = await _context.Shipments.ToListAsync();

            var distribution = new Dictionary<string, int>
            {
                { "Processing", 0 },
                { "InTransit", 0 },
                { "Delivered", 0 },
                { "Delayed", 0 }
            };

            foreach ( var shipment in shipments )
            {
                switch ( shipment.Status )
                {
                    case ShipmentStatus.Processing:
                        distribution["Processing"]++;
                        break;
                    case ShipmentStatus.InTransit:
                        distribution["InTransit"]++;
                        break;
                    case ShipmentStatus.Delivered:
                        distribution["Delivered"]++;
                        break;
                    case ShipmentStatus.Delayed:
                        distribution["Delayed"]++;
                        break;
                }
            }

            return ShipmentStatusDistributionResult.Success( distribution );
        }
        catch ( Exception ex )
        {
            return ShipmentStatusDistributionResult.Failure( $"Error calculating status distribution: {ex.Message}" );
        }
    }
}