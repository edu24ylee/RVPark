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
        public DbSet<ReservationUpdateModel> ReservationUpdateModel { get; set; }
        public DbSet<Guest> Guest { get; set; }
        public DbSet<Rv> RV { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<ReservationReport> ReservationReport { get; set; }
        public DbSet<Fee> Fee { get; set; }
        public DbSet<FeeType> FeeType { get; set; }
        public DbSet<FinancialReport> FinancialReport { get; set; }
        public DbSet<Policy> Policy { get; set; }
        public DbSet<DodAffiliation> DodAffiliation { get; set; }

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

            modelBuilder.Entity<Rv>()
                .HasOne(rv => rv.Guest)
                .WithMany(g => g.RVs)
                .HasForeignKey(rv => rv.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Guest>()
                .HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinancialReport>().HasNoKey();

            modelBuilder.Entity<Fee>()
                .Property(f => f.FeeTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FinancialReport>()
                .Property(fr => fr.AnticipatedRevenue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FinancialReport>()
                .Property(fr => fr.CollectedRevenue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReservationUpdateModel>().HasNoKey();
        }
    }
}
