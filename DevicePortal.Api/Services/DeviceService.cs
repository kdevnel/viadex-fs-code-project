using DevicePortal.Api.Data;
using DevicePortal.Api.Models;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DevicePortal.Api.Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _db;

    public DeviceService( AppDbContext db )
    {
        _db = db;
    }

    public async Task<DevicePagedResult> GetDevicesAsync( int page, int pageSize, DeviceStatus? status = null )
    {
        // Business logic: Validation rules
        if ( page <= 0 || pageSize <= 0 )
            return DevicePagedResult.Failure( "Page and pageSize must be positive" );

        if ( pageSize > 100 )
            return DevicePagedResult.Failure( "Page size cannot exceed 100" );

        try
        {
            // Business logic: Data access and calculations
            var query = _db.Devices.AsQueryable();

            // Apply status filter if provided
            if ( status.HasValue )
                query = query.Where( d => d.Status == status.Value );

            var total = await query.CountAsync();
            var items = await query
                .OrderBy( d => d.Id )
                .Skip( (page - 1) * pageSize )
                .Take( pageSize )
                .ToListAsync();

            return DevicePagedResult.Success( total, items );
        }
        catch ( Exception ex )
        {
            // Business logic: Error handling
            return DevicePagedResult.Failure( $"Error retrieving devices: {ex.Message}" );
        }
    }

    public async Task<Device?> GetDeviceByIdAsync( int id )
    {
        // Business logic: Input validation
        if ( id <= 0 )
            return null;

        try
        {
            // Business logic: Data access
            return await _db.Devices.FindAsync( id );
        }
        catch
        {
            // Business logic: Error handling
            return null;
        }
    }

    public async Task<DeviceCreateResult> CreateDeviceAsync( Device device )
    {
        try
        {
            // Business logic: Complex validation
            if ( string.IsNullOrWhiteSpace( device.Name ) )
                return DeviceCreateResult.Failure( "Device name is required" );

            if ( device.Name.Length > 100 )
                return DeviceCreateResult.Failure( "Device name cannot exceed 100 characters" );

            if ( device.MonthlyPrice <= 0 )
                return DeviceCreateResult.Failure( "Monthly price must be positive" );

            if ( device.MonthlyPrice > 10000 )
                return DeviceCreateResult.Failure( "Monthly price cannot exceed £10,000" );

            // Business logic: Check for duplicates
            var existingDevice = await _db.Devices
                .FirstOrDefaultAsync( d => d.Name.ToLower() == device.Name.ToLower() );

            if ( existingDevice != null )
                return DeviceCreateResult.Failure( "Device with this name already exists" );

            // Business logic: Set audit fields
            device.PurchaseDate = DateTime.UtcNow;

            // Business logic: Data persistence
            _db.Devices.Add( device );
            await _db.SaveChangesAsync();

            return DeviceCreateResult.Success( device );
        }
        catch ( Exception ex )
        {
            return DeviceCreateResult.Failure( $"Error creating device: {ex.Message}" );
        }
    }

    public async Task<DeviceDeleteResult> DeleteDeviceAsync( int id )
    {
        try
        {
            // Input validation
            if ( id <= 0 )
                return DeviceDeleteResult.Failure( "Invalid device ID" );

            // Data access
            var device = await _db.Devices.FindAsync( id );
            if ( device == null )
                return DeviceDeleteResult.Failure( "Device not found" );

            // Data deletion
            _db.Devices.Remove( device );
            await _db.SaveChangesAsync();

            return DeviceDeleteResult.Success();
        }
        catch ( Exception ex )
        {
            return DeviceDeleteResult.Failure( $"Error deleting device: {ex.Message}" );
        }
    }

    public async Task<DeviceStatusDistributionResult> GetDeviceStatusDistributionAsync()
    {
        try
        {
            // Business logic: Calculate status distribution
            var distribution = await _db.Devices
                .GroupBy( d => d.Status )
                .ToDictionaryAsync( g => g.Key, g => g.Count() );

            return DeviceStatusDistributionResult.Success( distribution );
        }
        catch ( Exception ex )
        {
            return DeviceStatusDistributionResult.Failure( $"Error getting status distribution: {ex.Message}" );
        }
    }
}