using ParcelAuthAPI.Models;

namespace ParcelAuthAPI.Data;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Parcel> Parcels { get; set; }
    public DbSet<Handover> Handovers { get; set; }
    public DbSet<TamperAlert> TamperAlerts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
