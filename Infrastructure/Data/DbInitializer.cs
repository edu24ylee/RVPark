using System;
using System.Linq;
using ApplicationCore.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Utilities;

namespace Infrastructure
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            _db.Database.EnsureCreated();

            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception)
            {
            }

            if (_db.Park.Any())
            {
                return;
            }

            _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.ManagerRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.GuestRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.MaintenanceRole)).GetAwaiter().GetResult();

            var parks = new Park[]
            {
                new Park { Name = "Sunset Valley RV Park", Address = "123 Sunset Blvd", City = "Mesa", State = "AZ", Zipcode = "85201" },
                new Park { Name = "Mountain View Campgrounds", Address = "456 Hilltop Dr", City = "Provo", State = "UT", Zipcode = "84604" }
            };
            _db.Park.AddRange(parks);
            _db.SaveChanges();

            var lotTypes = new LotType[]
            {
                new LotType { Name = "Standard", Rate = 30.00, ParkId = parks[0].Id },
                new LotType { Name = "Premium", Rate = 45.00, ParkId = parks[0].Id }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            var lots = new Lot[]
            {
                new Lot { Location = "A1", Length = 30, Width = 10, HeightLimit = 12, IsAvailable = true, Description = "Shaded near restroom", LotTypeId = lotTypes[0].Id },
                new Lot { Location = "B2", Length = 45, Width = 12, HeightLimit = 14, IsAvailable = true, Description = "Sunny, next to office", LotTypeId = lotTypes[1].Id }
            };
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            var guestUser = new User
            {
                Email = "guest1@example.com",
                Password = "password123",
                FirstName = "Jane",
                LastName = "Doe",
                Phone = "8015556789",
                IsActive = true
            };
            _db.User.Add(guestUser);
            _db.SaveChanges();

            var guest = new Guest
            {
                UserID = guestUser.UserID,
                DodId = 123456
            };
            _db.Guest.Add(guest);
            _db.SaveChanges();

            var rv = new RV
            {
                GuestID = guest.GuestID,
                LicensePlate = "RV123",
                Length = 30,
                Make = "Winnebago",
                Model = "Adventurer",
                Description = "Sleeps 4"
            };
            _db.RV.Add(rv);
            _db.SaveChanges();

            var reservation = new Reservation
            {
                GuestId = guest.GuestID,
                RvId = rv.RvID,
                LotId = lots[0].Id,
                Duration = 5,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Status = SD.StatusActive
            };
            _db.Reservation.Add(reservation);
            _db.SaveChanges();

            var report = new ReservationReport
            {
                GuestName = guestUser.FirstName + " " + guestUser.LastName,
                Email = guestUser.Email,
                Phone = guestUser.Phone,
                RVMake = rv.Make,
                RVModel = rv.Model,
                LicensePlate = rv.LicensePlate,
                TrailerLength = rv.Length,
                LotLocation = lots[0].Location,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Duration = reservation.Duration,
                Status = reservation.Status,
                TotalPaid = (decimal)(reservation.Duration * lotTypes[0].Rate)
            };
            _db.ReservationReport.Add(report);
            _db.SaveChanges();

            var employeeUser = new User
            {
                Email = "employee1@rvpark.com",
                Password = "employee123",
                FirstName = "John",
                LastName = "Smith",
                Phone = "8015551234",
                IsActive = true
            };
            _db.User.Add(employeeUser);
            _db.SaveChanges();

            var employee = new Employee(employeeUser, 1);
            _db.Employee.Add(employee);
            _db.SaveChanges();

            var feeTypes = new List<FeeType>
            {
                new FeeType { FeeTypeName = "Late Fee" },
                new FeeType { FeeTypeName = "Cleaning Fee" },
                new FeeType { FeeTypeName = "Pet Fee" }
            };
            _db.FeeType.AddRange(feeTypes);
            _db.SaveChanges();

            var fees = new List<Fee>
            {
                new Fee { FeeTypeId = feeTypes[0].Id, FeeTotal = 25.00m },
                new Fee { FeeTypeId = feeTypes[1].Id, FeeTotal = 50.00m },
                new Fee { FeeTypeId = feeTypes[2].Id, FeeTotal = 15.00m }
            };
            _db.Fee.AddRange(fees);
            _db.SaveChanges();
        }
    }
}
