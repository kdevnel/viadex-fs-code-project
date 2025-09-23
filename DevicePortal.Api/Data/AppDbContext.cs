using Microsoft.EntityFrameworkCore;
using DevicePortal.Api.Models;

namespace DevicePortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext( DbContextOptions<AppDbContext> options ) : base( options ) { }
    public DbSet<Device> Devices => Set<Device>();
    protected override void OnModelCreating( ModelBuilder b )
    {
        b.Entity<Device>().Property( d => d.MonthlyPrice ).HasColumnType( "decimal(7,2)" );
        b.Entity<Device>().Property( d => d.Status ).HasConversion<int>();
    }
}