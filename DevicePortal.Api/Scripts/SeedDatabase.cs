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

    public static async Task SeedQuotesAsync( AppDbContext context )
    {
        // Check if quotes already exist
        if ( await context.Quotes.AnyAsync() )
        {
            Console.WriteLine( "Database already contains quotes. Skipping seed." );
            return;
        }

        Console.WriteLine( "Seeding database with sample quotes..." );

        // Get existing devices for quote relationships
        var devices = await context.Devices.Take( 5 ).ToListAsync();
        if ( !devices.Any() )
        {
            Console.WriteLine( "No devices found. Please seed devices first." );
            return;
        }

        var random = new Random();
        var customerNames = new[]
        {
            "James Thompson", "Sarah Williams", "Michael Davies", "Emily Clarke",
            "Robert Roberts", "Lisa Jones", "David Wilson", "Emma Brown",
            "Tom Anderson", "Sophie Taylor", "Alex Miller", "Kate Johnson"
        };

        var quotes = new List<Quote>();

        foreach ( var device in devices )
        {
            // Create 2-3 quotes per device with different configurations
            var quotesPerDevice = random.Next( 2, 4 );

            for ( var i = 0; i < quotesPerDevice; i++ )
            {
                var customerName = customerNames[random.Next( customerNames.Length )];
                var durationMonths = new[] { 6, 12, 18, 24, 36 }[random.Next( 5 )];
                var supportTier = ( SupportTier ) random.Next( 1, 4 );

                // Calculate pricing
                var monthlyRate = device.MonthlyPrice;
                var supportMultiplier = supportTier switch {
                    SupportTier.Basic => 0.0m,
                    SupportTier.Standard => 0.2m,
                    SupportTier.Premium => 0.5m,
                    _ => 0.0m
                };

                var supportRate = monthlyRate * supportMultiplier;
                var totalMonthlyCost = monthlyRate + supportRate;
                var totalCost = totalMonthlyCost * durationMonths;

                var quote = new Quote {
                    DeviceId = device.Id,
                    CustomerName = customerName,
                    DurationMonths = durationMonths,
                    SupportTier = supportTier,
                    MonthlyRate = monthlyRate,
                    SupportRate = supportRate,
                    TotalMonthlyCost = totalMonthlyCost,
                    TotalCost = totalCost,
                    CreatedAt = DateTime.UtcNow.AddDays( -random.Next( 1, 30 ) ),
                    ValidUntil = DateTime.UtcNow.AddDays( random.Next( 10, 60 ) )
                };

                quotes.Add( quote );
            }
        }

        await context.Quotes.AddRangeAsync( quotes );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully seeded {quotes.Count} quotes to the database." );
    }

    public static async Task ClearQuotesAsync( AppDbContext context )
    {
        Console.WriteLine( "Clearing all quotes from the database..." );

        var quoteCount = await context.Quotes.CountAsync();

        if ( quoteCount == 0 )
        {
            Console.WriteLine( "Database is already empty. No quotes to clear." );
            return;
        }

        // Remove all quotes
        var quotes = await context.Quotes.ToListAsync();
        context.Quotes.RemoveRange( quotes );
        await context.SaveChangesAsync();

        Console.WriteLine( $"Successfully cleared {quoteCount} quotes from the database." );
    }

    public static async Task SeedAllAsync( AppDbContext context )
    {
        Console.WriteLine( "Seeding all data (devices + shipments + quotes)..." );

        await SeedDevicesAsync( context );
        await SeedShipmentsAsync( context );
        await SeedQuotesAsync( context );

        Console.WriteLine( "All seeding completed." );
    }

    public static async Task ClearAllAsync( AppDbContext context )
    {
        Console.WriteLine( "Clearing all data (devices + shipments + quotes)..." );

        await ClearQuotesAsync( context );  // Clear quotes first due to foreign key constraints
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
        Console.WriteLine( "Reseeding all data (clear + seed devices + shipments + quotes)..." );

        await ClearAllAsync( context );
        await SeedAllAsync( context );

        Console.WriteLine( "Complete reseed operation completed." );
    }

    public static async Task ReseedQuotesAsync( AppDbContext context )
    {
        Console.WriteLine( "Reseeding quotes (clear + seed)..." );

        await ClearQuotesAsync( context );
        await SeedQuotesAsync( context );

        Console.WriteLine( "Quote reseed operation completed." );
    }
}