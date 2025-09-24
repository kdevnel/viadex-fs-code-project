using Microsoft.EntityFrameworkCore;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext( DbContextOptions<AppDbContext> options ) : base( options ) { }
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating( ModelBuilder b )
    {
        // Device configuration
        b.Entity<Device>().Property( d => d.MonthlyPrice ).HasColumnType( "decimal(7,2)" );
        b.Entity<Device>().Property( d => d.Status ).HasConversion<int>();

        // Shipment configuration
        b.Entity<Shipment>().Property( s => s.Status ).HasConversion<int>();
        b.Entity<Shipment>().HasIndex( s => s.TrackingNumber ).IsUnique();

        // Quote configuration
        b.Entity<Quote>().Property( q => q.MonthlyRate ).HasColumnType( "decimal(7,2)" );
        b.Entity<Quote>().Property( q => q.SupportRate ).HasColumnType( "decimal(7,2)" );
        b.Entity<Quote>().Property( q => q.TotalMonthlyCost ).HasColumnType( "decimal(7,2)" );
        b.Entity<Quote>().Property( q => q.TotalCost ).HasColumnType( "decimal(10,2)" );
        b.Entity<Quote>().Property( q => q.SupportTier ).HasConversion<int>();

        // Quote-Device relationship
        b.Entity<Quote>()
            .HasOne( q => q.Device )
            .WithMany()
            .HasForeignKey( q => q.DeviceId )
            .OnDelete( DeleteBehavior.Restrict );
    }
}