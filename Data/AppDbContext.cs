using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Models;

namespace ParcelAuthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<Handover> Handovers { get; set; }
        public DbSet<TamperAlert> TamperAlerts { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<ParcelStatusLog> ParcelStatusLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Email).HasColumnType("TEXT");
                entity.Property(u => u.PasswordHash).HasColumnType("TEXT");
                entity.Property(u => u.Role).HasColumnType("TEXT");
            });
        
            modelBuilder.Entity<Parcel>(entity =>
            {
                entity.Property(p => p.SenderEmail).HasColumnType("TEXT");
                entity.Property(p => p.ReceiverEmail).HasColumnType("TEXT");
                
            });

           
        }
    }
}