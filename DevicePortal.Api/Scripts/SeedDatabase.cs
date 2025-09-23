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

    public static async Task ReseedDevicesAsync( AppDbContext context )
    {
        Console.WriteLine( "Reseeding database (clear + seed)..." );

        await ClearDevicesAsync( context );
        await SeedDevicesAsync( context );

        Console.WriteLine( "Reseed operation completed." );
    }
}