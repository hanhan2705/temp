using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestDevice> RequestDevices { get; set; }
        public DbSet<AllocationHistory> AllocationHistories { get; set; }
        public DbSet<ReturnHistory> ReturnHistories { get; set; }
        public DbSet<Depreciation> Depreciations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for RequestDevice
            modelBuilder.Entity<RequestDevice>()
                .HasKey(rd => new { rd.RequestId, rd.DeviceId });

            // Unique index for Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 1-1 mapping Device - Depreciation (Depreciation is dependent on Device)
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Depreciation)
                .WithOne(dp => dp.Device)
                .HasForeignKey<Depreciation>(dp => dp.DeviceId);
        }
    }
}