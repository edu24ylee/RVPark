using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents;
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

            if (!_db.Park.Any() && !_db.Users.Any())
            {
                SeedData(); // ✅ This method now contains all the logic
            }
        }

        private void SeedData()
        {
            if (_db.Park.Any()) return; // double check

            // 🔽 All your original seeding logic goes here
            var roles = new[] { SD.AdminRole, SD.ManagerRole, SD.SuperAdminRole, SD.GuestRole, SD.MaintenanceRole, SD.CampHostRole };

            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            var superEmail = "tawnymcaleese@gmail.com";
            var superUser = _userManager.FindByEmailAsync(superEmail).GetAwaiter().GetResult();
            if (superUser == null)
            {
                superUser = new IdentityUser { UserName = superEmail, Email = superEmail, EmailConfirmed = true };
                var result = _userManager.CreateAsync(superUser, "Admin123*").GetAwaiter().GetResult();
                if (!result.Succeeded)
                    throw new Exception("Failed to create SuperAdmin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!_userManager.IsInRoleAsync(superUser, SD.SuperAdminRole).GetAwaiter().GetResult())
            {
                _userManager.AddToRoleAsync(superUser, SD.SuperAdminRole).GetAwaiter().GetResult();
            }

            var customUser = _db.User.FirstOrDefault(u => u.Email.ToLower() == superEmail.ToLower());
            if (customUser == null)
            {
                customUser = new User
                {
                    FirstName = "Tawny",
                    LastName = "McAleese",
                    Email = superEmail,
                    Phone = "555-9999",
                    IsActive = true,
                    IdentityUserId = superUser.Id
                };
                _db.User.Add(customUser);
                _db.SaveChanges();
            }

            if (!_db.Employee.Any(e => e.UserID == customUser.UserID))
            {
                var emp = new Employee { UserID = customUser.UserID, Role = SD.SuperAdminRole };
                _db.Employee.Add(emp);
                _db.SaveChanges();
            }

            var adminEmail = SD.DefaultAdminEmail;
            var adminUser = _userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = _userManager.CreateAsync(adminUser, SD.DefaultPassword).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(adminUser, SD.AdminRole).GetAwaiter().GetResult();
                }
            }

            var park = new Park
            {
                Name = "Desert Eagle Nellis AFB",
                Address = "4707 Duffer Dr",
                City = "Las Vegas",
                State = "NV",
                Zipcode = "89191"
            };
            _db.Park.Add(park);
            _db.SaveChanges();

            var today = DateTime.Today;
            var lotTypes = new List<LotType>
            {
                new LotType { Name = "Standard", Rate = 40.00, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1), IsArchived = false },
                new LotType { Name = "Premium", Rate = 55.00, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1), IsArchived = false },
                new LotType { Name = "Deluxe", Rate = 70.00, ParkId = park.Id, StartDate = today, EndDate = today.AddYears(1), IsArchived = false }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

           var feeTypes = new List<FeeType>
            {
                new FeeType
                {
                    FeeTypeName = "Late Cancellation Fee",
                    Description = "Applies when cancellation occurs too close to reservation start.",
                    TriggerType = TriggerType.Triggered,
                    TriggerRuleJson = "{\"DaysBefore\": 3, \"PenaltyPercent\": 50}",
                    IsArchived = false
                },
                new FeeType
                {
                    FeeTypeName = "Pet Fee",
                    Description = "Applied when guests bring pets.",
                    TriggerType = TriggerType.Manual,
                    TriggerRuleJson = null,
                    IsArchived = false
                },
                new FeeType
                {
                    FeeTypeName = "Damage Fee",
                    Description = "Manually applied for property damage.",
                    TriggerType = TriggerType.Manual,
                    TriggerRuleJson = null,
                    IsArchived = false
                },
                new FeeType
                {
                    FeeTypeName = "Extra Vehicle Fee",
                    Description = "Manually applied if guests bring more than one vehicle.",
                    TriggerType = TriggerType.Manual,
                    TriggerRuleJson = null,
                    IsArchived = false
                },
                new FeeType
                {
                    FeeTypeName = "Holiday Premium",
                    Description = "Automatically increases rate during holidays.",
                    TriggerType = TriggerType.Triggered,
                    TriggerRuleJson = "{\"HolidayRateMultiplier\": 1.25}",
                    IsArchived = false
                }
            };
            _db.FeeType.AddRange(feeTypes);
            _db.SaveChanges();

            var lots = new List<Lot>();
            foreach (var lt in lotTypes)
            {
                for (int i = 1; i <= 5; i++)
                {
                    lots.Add(new Lot
                    {
                        LotTypeId = lt.Id,
                        Description = $"{lt.Name} Lot {i}",
                        Length = new[] { 30.0, 35.0, 40.0 }[new Random().Next(0, 3)],
                        Width = 20,
                        Location = $"{lt.Name[0]}{i}",
                        IsAvailable = true
                    });
                }
            }
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            var characterNames = new (string FirstName, string LastName)[]
            {
                ("Sheldon", "Cooper"),
                ("Leonard", "Hofstadter"),
                ("Penny", "Teller"),
                ("Howard", "Wolowitz"),
                ("Raj", "Koothrappali")
            };
            var statuses = new[] { SD.StatusPending, SD.StatusConfirmed, SD.StatusActive, SD.StatusCancelled, SD.StatusCompleted };
            var rand = new Random();

            for (int i = 0; i < characterNames.Length; i++)
            {
                var (first, last) = characterNames[i];
                var email = $"guest{i + 1}@email.com";
                var guestIdentity = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

                var result = _userManager.CreateAsync(guestIdentity, "Guest123!").GetAwaiter().GetResult();
                if (!result.Succeeded)
                    throw new Exception("Failed to create guest: " + string.Join(", ", result.Errors.Select(e => e.Description)));

                var user = new User
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = $"555-000{i + 1}",
                    IsActive = true,
                    IdentityUserId = guestIdentity.Id
                };
                _db.User.Add(user);
                _db.SaveChanges();

                var guest = new Guest { DodId = 2000 + i, UserID = user.UserID };
                _db.Guest.Add(guest);
                _db.SaveChanges();

                var rv = new RV
                {
                    GuestID = guest.GuestID,
                    Description = $"RV Model {i + 1}",
                    Length = 30 + i,
                    Make = "Winnebago",
                    Model = $"Model-{i + 1}",
                    LicensePlate = $"RV-00{i + 1}"
                };
                _db.RV.Add(rv);
                _db.SaveChanges();

                for (int j = 0; j < 3; j++)
                {
                    var startOffset = rand.Next(-20, 10);
                    var duration = rand.Next(2, 7);
                    var startDate = today.AddDays(startOffset);
                    var endDate = startDate.AddDays(duration);
                    var status = statuses[rand.Next(statuses.Length)];
                    var lot = lots[rand.Next(lots.Count)];

                    var reservation = new Reservation
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Duration = duration,
                        Status = status,
                        GuestId = guest.GuestID,
                        LotId = lot.Id,
                        RvId = rv.RvID,
                        OverrideReason = "Seeded for testing",
                        CancellationDate = status == SD.StatusCancelled ? endDate.AddDays(-1) : null,
                        CancellationReason = status == SD.StatusCancelled ? "No longer needed" : null
                    };
                    _db.Reservation.Add(reservation);
                    _db.SaveChanges();
                }
            }

            var employees = new List<(string Email, string First, string Last, string Role)>
            {
                ("janet@rvpark.com", "Janet", "Walker", SD.ManagerRole),
                ("tom@rvpark.com", "Tom", "Barnes", SD.MaintenanceRole),
                ("maria@rvpark.com", "Maria", "Gonzalez", SD.GuestRole)
            };

            foreach (var (email, first, last, role) in employees)
            {
                var idUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = _userManager.CreateAsync(idUser, "Emp123!").GetAwaiter().GetResult();
                if (!result.Succeeded)
                    throw new Exception("Failed to create employee: " + string.Join(", ", result.Errors.Select(e => e.Description)));

                var user = new User
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = "555-0100",
                    IsActive = true,
                    IdentityUserId = idUser.Id
                };
                _db.User.Add(user);
                _db.SaveChanges();

                var emp = new Employee { UserID = user.UserID, Role = role };
                _db.Employee.Add(emp);
                _db.SaveChanges();
            }
        }
    }
}
