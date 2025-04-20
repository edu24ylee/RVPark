// Seed Requirements
// - One admin and one employee (Tawny(Admin) and Christian(Employee))
// - One seed guest for all reservations
// - All lot types and fees
// - 2–3 lots per type
// - At least 12 reservations with varied statuses and dates

using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TriggerType = ApplicationCore.Models.TriggerType;

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
            _db.Database.Migrate();
            SeedData();
        }

        private void SeedData()
        {
            if (_db.Park.Any()) return;

            var roles = new[] { SD.AdminRole, SD.ManagerRole, SD.SuperAdminRole, SD.GuestRole, SD.MaintenanceRole, SD.CampHostRole };
            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            var superEmail = "tawnymcaleese@gmail.com";
            var superUser = _userManager.FindByEmailAsync(superEmail).GetAwaiter().GetResult();
            if (superUser == null)
            {
                superUser = new IdentityUser { UserName = superEmail, Email = superEmail, EmailConfirmed = true };
                var result = _userManager.CreateAsync(superUser, "Admin123*").GetAwaiter().GetResult();
                if (!result.Succeeded) throw new Exception("Failed to create SuperAdmin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            _userManager.AddToRoleAsync(superUser, SD.SuperAdminRole).GetAwaiter().GetResult();

            var superDbUser = _db.User.FirstOrDefault(u => u.Email.ToLower() == superEmail.ToLower()) ?? new User
            {
                FirstName = "Tawny",
                LastName = "McAleese",
                Email = superEmail,
                Phone = "555-9999",
                IsActive = true,
                IdentityUserId = superUser.Id
            };
            if (superDbUser.UserId == 0)
            {
                _db.User.Add(superDbUser);
                _db.SaveChanges();
            }
            if (!_db.Employee.Any(e => e.UserId == superDbUser.UserId))
            {
                _db.Employee.Add(new Employee { UserId = superDbUser.UserId, Role = SD.SuperAdminRole });
                _db.SaveChanges();
            }

            var empEmail = "christianmartin@mail.weber.edu";
            var empUser = _userManager.FindByEmailAsync(empEmail).GetAwaiter().GetResult();
            if (empUser == null)
            {
                empUser = new IdentityUser { UserName = empEmail, Email = empEmail, EmailConfirmed = true };
                var empResult = _userManager.CreateAsync(empUser, "Employee123!").GetAwaiter().GetResult();
                if (!empResult.Succeeded) throw new Exception("Failed to create employee: " + string.Join(", ", empResult.Errors.Select(e => e.Description)));
                _userManager.AddToRoleAsync(empUser, SD.ManagerRole).GetAwaiter().GetResult();
            }

            var empDbUser = _db.User.FirstOrDefault(u => u.Email.ToLower() == empEmail.ToLower()) ?? new User
            {
                FirstName = "Christian",
                LastName = "Martin",
                Email = empEmail,
                Phone = "555-555-5555",
                IsActive = true,
                IdentityUserId = empUser.Id
            };
            if (empDbUser.UserId == 0)
            {
                _db.User.Add(empDbUser);
                _db.SaveChanges();
            }
            if (!_db.Employee.Any(e => e.UserId == empDbUser.UserId))
            {
                _db.Employee.Add(new Employee { UserId = empDbUser.UserId, Role = SD.ManagerRole });
                _db.SaveChanges();
            }

            var park = new Park { Name = "Desert Eagle Nellis AFB", Address = "4707 Duffer Dr", City = "Las Vegas", State = "NV", Zipcode = "89191" };
            _db.Park.Add(park);
            _db.SaveChanges();

            var today = DateTime.Today;
            var lotTypes = new[]
            {
                new LotType { Name = "Standard", Rate = 40, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1) },
                new LotType { Name = "Premium", Rate = 55, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1) },
                new LotType { Name = "Deluxe", Rate = 70, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1) }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            var lots = lotTypes.SelectMany(lt => Enumerable.Range(1, 3).Select(i => new Lot
            {
                LotTypeId = lt.Id,
                Location = $"{lt.Name[0]}-{i}",
                Width = 20,
                Length = 35,
                IsAvailable = true
            })).ToList();
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            _db.FeeType.AddRange(new[]
            {
                new FeeType { FeeTypeName = "24 Hour Cancellation Fee", Policy = "Triggered if within 24 hrs", TriggerType = TriggerType.Triggered, TriggerRuleJson = "{\"HoursBefore\":24,\"PenaltyPercent\":100}" },
                new FeeType { FeeTypeName = "Extra Adults Fee", Policy = "Triggered per adult over 3", TriggerType = TriggerType.Triggered, TriggerRuleJson = "{\"Threshold\":3,\"Fee\":1.0}" },
                new FeeType { FeeTypeName = "Pet Cleanup Violation", Policy = "Manual fee for uncleaned pet waste", TriggerType = TriggerType.Manual }
            });
            _db.SaveChanges();

            var identityUser = new IdentityUser { UserName = "jessica.wells@rv.com", Email = "jessica.wells@rv.com", EmailConfirmed = true };
            var userResult = _userManager.CreateAsync(identityUser, "Guest123!").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(identityUser, SD.GuestRole).GetAwaiter().GetResult();

            var guestUser = new User { FirstName = "Jessica", LastName = "Wells", Email = "jessica.wells@rv.com", Phone = "555-0102", IdentityUserId = identityUser.Id, IsActive = true };
            _db.User.Add(guestUser);
            _db.SaveChanges();

            var guest = new Guest { UserId = guestUser.UserId, DodId = 8723 };
            _db.Guest.Add(guest);
            _db.SaveChanges();

            var rv = new Rv
            {
                GuestId = guest.GuestId,
                LicensePlate = "NEV-4527",
                Make = "Forest River",
                Model = "Wildwood X-Lite",
                Description = "2021 travel trailer with queen bed and bunks",
                Length = 32
            };
            _db.RV.Add(rv);
            _db.SaveChanges();


            var statuses = new[] { SD.StatusPending, SD.StatusConfirmed, SD.StatusActive, SD.StatusCancelled, SD.StatusCompleted };
            var reservations = new List<Reservation>();
            for (int i = 0; i < 12; i++)
            {
                reservations.Add(new Reservation
                {
                    GuestId = guest.GuestId,
                    RvId = rv.RvId,
                    LotId = lots[i % lots.Count].Id,
                    StartDate = today.AddDays(-10 + i),
                    EndDate = today.AddDays(-10 + i + 3),
                    Duration = 3,
                    Status = statuses[i % statuses.Length],
                    NumberOfAdults = 2 + (i % 4),
                    NumberOfPets = i % 2,
                    OverrideReason = i % 4 == 0 ? "Test override" : null
                });
            }
            _db.Reservation.AddRange(reservations);
            _db.SaveChanges();
        }
    }
}
