using Microsoft.EntityFrameworkCore;
using DevicePortal.Api.Data;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.Scripts;

public static class SeedDatabase
{
    public static async Task SeedDevicesAsync( AppDbContext context )
    {
        // Check if devices already exist
        if ( await context.Devices.AnyAsync() )
        {
            Console.WriteLine( "Database already contains devices. Skipping seed." );
            return;
        }

        Console.WriteLine( "Seeding database with sample devices..." );

        var devices = new List<Device>
        {
            new Device
            {
                Name = "iPhone 15 Pro",
                Model = "A3102",
                MonthlyPrice = 45.99m,
                PurchaseDate = new DateTime(2024, 1, 15),
                Status = DeviceStatus.Active
            },
            new Device
            {
                Name = "Samsung Galaxy S24",
                Model = "SM-S921B",
                MonthlyPrice = 42.50m,
                PurchaseDate = new DateTime(2025, 2, 20),
                Status = DeviceStatus.Active
            },
            new Device
            {
                Name = "iPad Air",
                Model = "A2316",
                MonthlyPrice = 25.99m,
                PurchaseDate = new DateTime(2024, 3, 10),
                Status = DeviceStatus.UnderRepair
            },
            new Device
            {
                Name = "MacBook Pro 14\"",
                Model = "M3",
                MonthlyPrice = 89.99m,
                PurchaseDate = new DateTime(2024, 1, 5),
                Status = DeviceStatus.Active
            },
            new Device
            {
                Name = "Dell XPS 13",
                Model = "9340",
                MonthlyPrice = 55.75m,
                PurchaseDate = new DateTime(2023, 4, 12),
                Status = DeviceStatus.Retired
            },
            new Device
            {
                Name = "Google Pixel 8",
                Model = "GC3VE",
                MonthlyPrice = 38.99m,
                PurchaseDate = new DateTime(2024, 2, 28),
                Status = DeviceStatus.Active
            }
        };

        await context.Devices.AddRangeAsync( devices );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully seeded {devices.Count} devices to the database." );
    }

    public static async Task ClearDevicesAsync( AppDbContext context )
    {
        Console.WriteLine( "Clearing all devices from the database..." );

        var deviceCount = await context.Devices.CountAsync();

        if ( deviceCount == 0 )
        {
            Console.WriteLine( "Database is already empty. No devices to clear." );
            return;
        }

        // Remove all devices
        var devices = await context.Devices.ToListAsync();
        context.Devices.RemoveRange( devices );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully cleared {deviceCount} devices from the database." );
    }

    public static async Task SeedShipmentsAsync( AppDbContext context )
    {
        // Check if shipments already exist
        if ( await context.Shipments.AnyAsync() )
        {
            Console.WriteLine( "Database already contains shipments. Skipping seed." );
            return;
        }

        Console.WriteLine( "Seeding database with sample shipments..." );

        var shipments = new List<Shipment>
        {
            new Shipment
            {
                TrackingNumber = "TRK001234567",
                CustomerName = "James Thompson",
                Status = ShipmentStatus.InTransit,
                EstimatedDelivery = DateTime.UtcNow.AddDays(2),
                Destination = "42 Baker Street, London, W1U 3AA",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Shipment
            {
                TrackingNumber = "TRK002345678",
                CustomerName = "Sarah Williams",
                Status = ShipmentStatus.Delivered,
                EstimatedDelivery = DateTime.UtcNow.AddDays(-1),
                ActualDelivery = DateTime.UtcNow.AddDays(-1),
                Destination = "15 Oak Road, Manchester, M14 7LG",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Shipment
            {
                TrackingNumber = "TRK003456789",
                CustomerName = "Michael Davies",
                Status = ShipmentStatus.Processing,
                EstimatedDelivery = DateTime.UtcNow.AddDays(5),
                Destination = "73 High Street, Edinburgh, EH1 1SR",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Shipment
            {
                TrackingNumber = "TRK004567890",
                CustomerName = "Emily Clarke",
                Status = ShipmentStatus.Delayed,
                EstimatedDelivery = DateTime.UtcNow.AddDays(3),
                Destination = "28 Mill Lane, Birmingham, B5 6RT",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Shipment
            {
                TrackingNumber = "TRK005678901",
                CustomerName = "David Brown",
                Status = ShipmentStatus.InTransit,
                EstimatedDelivery = DateTime.UtcNow.AddDays(1),
                Destination = "89 Castle Street, Cardiff, CF10 1BU",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Shipment
            {
                TrackingNumber = "TRK006789012",
                CustomerName = "Lisa Roberts",
                Status = ShipmentStatus.Delivered,
                EstimatedDelivery = DateTime.UtcNow.AddDays(-3),
                ActualDelivery = DateTime.UtcNow.AddDays(-2),
                Destination = "156 Queen's Road, Brighton, BN1 3XG",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        await context.Shipments.AddRangeAsync( shipments );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully seeded {shipments.Count} shipments to the database." );
    }

    public static async Task ClearShipmentsAsync( AppDbContext context )
    {
        Console.WriteLine( "Clearing all shipments from the database..." );

        var shipmentCount = await context.Shipments.CountAsync();

        if ( shipmentCount == 0 )
        {
            Console.WriteLine( "Database is already empty. No shipments to clear." );
            return;
        }

        // Remove all shipments
        var shipments = await context.Shipments.ToListAsync();
        context.Shipments.RemoveRange( shipments );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully cleared {shipmentCount} shipments from the database." );
    }

    public static async Task SeedAllAsync( AppDbContext context )
    {
        Console.WriteLine( "Seeding all data (devices + shipments)..." );

        await SeedDevicesAsync( context );
        await SeedShipmentsAsync( context );

        Console.WriteLine( "All seeding completed." );
    }

    public static async Task ClearAllAsync( AppDbContext context )
    {
        Console.WriteLine( "Clearing all data (devices + shipments)..." );

        await ClearDevicesAsync( context );
        await ClearShipmentsAsync( context );

        Console.WriteLine( "All data cleared." );
    }

    public static async Task ReseedDevicesAsync( AppDbContext context )
    {
        Console.WriteLine( "Reseeding database (clear + seed)..." );

        await ClearDevicesAsync( context );
        await SeedDevicesAsync( context );

        Console.WriteLine( "Reseed operation completed." );
    }

    public static async Task ReseedAllAsync( AppDbContext context )
    {
        Console.WriteLine( "Reseeding all data (clear + seed devices + shipments)..." );

        await ClearAllAsync( context );
        await SeedAllAsync( context );

        Console.WriteLine( "Complete reseed operation completed." );
    }
}