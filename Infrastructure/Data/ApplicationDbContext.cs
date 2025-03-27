using ApplicationCore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Park> Park { get; set; }
        public DbSet<LotType> LotType { get; set; }
        public DbSet<Lot> Lot { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Guest> Guest { get; set; }
        public DbSet<RV> RV { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<ReservationReport> ReservationReport { get; set; }
        public DbSet<Fee> Fee { get; set; }
        public DbSet<FeeType> FeeType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Lot)
                .WithMany()
                .HasForeignKey(r => r.LotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Rv)
                .WithMany()
                .HasForeignKey(r => r.RvId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RV>()
                .HasOne(rv => rv.Guest)
                .WithMany(g => g.RVs)
                .HasForeignKey(rv => rv.GuestID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
