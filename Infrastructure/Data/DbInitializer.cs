using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            if (_db.Database.GetPendingMigrations().Any())
            {
                _db.Database.Migrate();
            }

            if (_db.Park.Any())
                return;

            // Create roles
            var roles = new[] { "Admin", "Manager", "SuperAdmin" };
            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            // Create admin user
            var adminUser = new IdentityUser
            {
                UserName = "admin@rvpark.com",
                Email = "admin@rvpark.com",
                EmailConfirmed = true
            };

            var adminResult = _userManager.CreateAsync(adminUser, "Password123!").GetAwaiter().GetResult();
            if (adminResult.Succeeded)
            {
                _userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }

            // Create or fetch SuperAdmin
            var adminEmail = "tawnymcaleese@gmail.com";
            var identityUser = _userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
            User customUser;

            if (identityUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(identityUser, "Admin123*").GetAwaiter().GetResult();
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create super admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                _userManager.AddToRoleAsync(identityUser, "SuperAdmin").GetAwaiter().GetResult();

                customUser = new User
                {
                    FirstName = "Tawny",
                    LastName = "McAleese",
                    Email = adminEmail,
                    Phone = "555-9999",
                    IsActive = true,
                    IdentityUserId = identityUser.Id
                };

                _db.User.Add(customUser);
                _db.SaveChanges();
            }
            else
            {
                customUser = _db.User.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower())!;
            }

            // Add Guest and RV for SuperAdmin if they don't exist
            if (!_db.Guest.Any(g => g.UserID == customUser.UserID))
            {
                var superGuest = new Guest
                {
                    DodId = 9999,
                    UserID = customUser.UserID
                };
                _db.Guest.Add(superGuest);
                _db.SaveChanges();

                var superRv = new RV
                {
                    GuestID = superGuest.GuestID,
                    Description = "SuperAdmin's RV",
                    Length = 38,
                    Make = "Airstream",
                    Model = "Classic XL",
                    LicensePlate = "SUPR-001"
                };
                _db.RV.Add(superRv);
                _db.SaveChanges();
            }

            // Add Park
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

            // Add LotTypes
            var lotTypes = new List<LotType>
            {
                new LotType { Name = "Standard", Rate = 40.00, ParkId = park.Id },
                new LotType { Name = "Premium", Rate = 55.00, ParkId = park.Id },
                new LotType { Name = "Deluxe", Rate = 70.00, ParkId = park.Id }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            // Add Lots
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

            // Guests from Big Bang Theory
            var characterNames = new (string FirstName, string LastName)[]
            {
                ("Sheldon", "Cooper"),
                ("Leonard", "Hofstadter"),
                ("Penny", "Teller"),
                ("Howard", "Wolowitz"),
                ("Raj", "Koothrappali")
            };

            var statuses = new[] { "Pending", "Confirmed", "Active", "Cancelled", "Completed" };
            var rand = new Random();
            var today = DateTime.Today;

            for (int i = 0; i < characterNames.Length; i++)
            {
                var (first, last) = characterNames[i];
                var email = $"guest{i + 1}@email.com";

                var guestIdentityUser = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var guestIdentityResult = _userManager.CreateAsync(guestIdentityUser, "Guest123!").GetAwaiter().GetResult();
                if (!guestIdentityResult.Succeeded)
                    throw new Exception("Failed to create guest IdentityUser: " + string.Join(", ", guestIdentityResult.Errors.Select(e => e.Description)));

                var user = new User
                {
                    Email = email,
                    FirstName = first,
                    LastName = last,
                    Phone = $"555-000{i + 1}",
                    IsActive = true,
                    IdentityUserId = guestIdentityUser.Id
                };
                _db.User.Add(user);
                _db.SaveChanges();

                var guest = new Guest
                {
                    DodId = 2000 + i,
                    UserID = user.UserID
                };
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
                        CancellationDate = status == "Cancelled" ? endDate.AddDays(-1) : null,
                        CancellationReason = status == "Cancelled" ? "No longer needed" : null
                    };
                    _db.Reservation.Add(reservation);
                    _db.SaveChanges();
                }
            }

            // Add Employees
            var employeeUsers = new List<(string Email, string FirstName, string LastName, string Role)>
            {
                ("janet@rvpark.com", "Janet", "Walker", "Manager"),
                ("tom@rvpark.com", "Tom", "Barnes", "Maintenance"),
                ("maria@rvpark.com", "Maria", "Gonzalez", "Guest")
            };

            foreach (var (email, first, last, role) in employeeUsers)
            {
                var identityUserEmp = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var empResult = _userManager.CreateAsync(identityUserEmp, "Emp123!").GetAwaiter().GetResult();
                if (!empResult.Succeeded)
                    throw new Exception("Failed to create employee IdentityUser: " + string.Join(", ", empResult.Errors.Select(e => e.Description)));

                var user = new User
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = "555-0100",
                    IsActive = true,
                    IdentityUserId = identityUserEmp.Id
                };
                _db.User.Add(user);
                _db.SaveChanges();

                var emp = new Employee
                {
                    UserID = user.UserID,
                    Role = role
                };
                _db.Employee.Add(emp);
                _db.SaveChanges();
            }
        }
    }
}
