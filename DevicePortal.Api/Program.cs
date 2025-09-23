using DevicePortal.Api.Data;
using DevicePortal.Api.Services;
using DevicePortal.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder( args );

// Database configuration
builder.Services.AddDbContext<AppDbContext>( o =>
    o.UseSqlServer( builder.Configuration.GetConnectionString( "DevicePortal" ) ) );

// Service registration
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// CORS configuration
builder.Services.AddCors( o => o.AddDefaultPolicy( p =>
    p.WithOrigins( "http://localhost:5173" ).AllowAnyHeader().AllowAnyMethod() ) );

// Controller configuration
builder.Services.AddControllers();

// API documentation (requires dotnet restore)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTPS redirection
builder.Services.AddHttpsRedirection( options => {
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 7027; // Match the HTTPS port from launchSettings.json
} );

var app = builder.Build();

// Configure the HTTP request pipeline
if ( app.Environment.IsDevelopment() )
{
    // Swagger will be available after package restore
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

// Check for database operation arguments
if ( args.Length > 0 )
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await context.Database.EnsureCreatedAsync();

    switch ( args[0].ToLower() )
    {
        case "seed":
            await DevicePortal.Api.Scripts.SeedDatabase.SeedAllAsync( context );
            Console.WriteLine( "Seeding completed. Exiting..." );
            break;

        case "seed-devices":
            await DevicePortal.Api.Scripts.SeedDatabase.SeedDevicesAsync( context );
            Console.WriteLine( "Device seeding completed. Exiting..." );
            break;

        case "seed-shipments":
            await DevicePortal.Api.Scripts.SeedDatabase.SeedShipmentsAsync( context );
            Console.WriteLine( "Shipment seeding completed. Exiting..." );
            break;

        case "clear":
            await DevicePortal.Api.Scripts.SeedDatabase.ClearAllAsync( context );
            Console.WriteLine( "Clear completed. Exiting..." );
            break;

        case "clear-devices":
            await DevicePortal.Api.Scripts.SeedDatabase.ClearDevicesAsync( context );
            Console.WriteLine( "Device clear completed. Exiting..." );
            break;

        case "clear-shipments":
            await DevicePortal.Api.Scripts.SeedDatabase.ClearShipmentsAsync( context );
            Console.WriteLine( "Shipment clear completed. Exiting..." );
            break;

        case "reseed":
            await DevicePortal.Api.Scripts.SeedDatabase.ReseedAllAsync( context );
            Console.WriteLine( "Reseed completed. Exiting..." );
            break;

        case "reseed-devices":
            await DevicePortal.Api.Scripts.SeedDatabase.ReseedDevicesAsync( context );
            Console.WriteLine( "Device reseed completed. Exiting..." );
            break;

        default:
            Console.WriteLine( $"Unknown command: {args[0]}" );
            Console.WriteLine( "Available commands:" );
            Console.WriteLine( "  seed, seed-devices, seed-shipments" );
            Console.WriteLine( "  clear, clear-devices, clear-shipments" );
            Console.WriteLine( "  reseed, reseed-devices" );
            break;
    }

    return;
}

app.Run();
