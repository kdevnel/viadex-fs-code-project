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

// CORS configuration
builder.Services.AddCors( o => o.AddDefaultPolicy( p =>
    p.WithOrigins( "http://localhost:5173" ).AllowAnyHeader().AllowAnyMethod() ) );

// Controller configuration
builder.Services.AddControllers();

// API documentation (requires dotnet restore)
builder.Services.AddEndpointsApiExplorer();

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

app.Run();
