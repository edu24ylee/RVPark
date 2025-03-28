using ApplicationCore.Models;
using ApplicationCore.Interfaces;
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

            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Migration failed: " + ex.Message);
            }

            if (_db.Park.Any())
                return;

            _roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole("Manager")).GetAwaiter().GetResult();

            var adminUser = new IdentityUser
            {
                UserName = "admin@rvpark.com",
                Email = "admin@rvpark.com",
                EmailConfirmed = true
            };

            _userManager.CreateAsync(adminUser, "Password123!").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();

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

            var lotTypes = new List<LotType>
            {
                new LotType { Name = "Standard", Rate = 40.00, ParkId = park.Id },
                new LotType { Name = "Premium", Rate = 55.00, ParkId = park.Id },
                new LotType { Name = "Deluxe", Rate = 70.00, ParkId = park.Id }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            var lot = new Lot
            {
                LotTypeId = lotTypes.First().Id,
                Description = "Pull-through lot near the entrance",
                Length = 35,
                Width = 20,
                HeightLimit = 14,
                Location = "A1",
                IsAvailable = true
            };
            _db.Lot.Add(lot);
            _db.SaveChanges();

            var feeTypes = new List<FeeType>
            {
                new FeeType { FeeTypeName = "Cleaning Fee" },
                new FeeType { FeeTypeName = "Late Check-Out Fee" },
                new FeeType { FeeTypeName = "Pet Fee" },
                new FeeType { FeeTypeName = "Extra Vehicle Fee" },
                new FeeType { FeeTypeName = "Reservation Change Fee" },
                new FeeType { FeeTypeName = "Damage Fee" },
                new FeeType { FeeTypeName = "Key Replacement Fee" }
            };
            _db.FeeType.AddRange(feeTypes);
            _db.SaveChanges();

            var fees = new List<Fee>
            {
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Cleaning Fee").Id, FeeTotal = 30.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Late Check-Out Fee").Id, FeeTotal = 20.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Pet Fee").Id, FeeTotal = 10.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Extra Vehicle Fee").Id, FeeTotal = 5.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Reservation Change Fee").Id, FeeTotal = 15.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Damage Fee").Id, FeeTotal = 100.00M },
                new Fee { FeeTypeId = feeTypes.First(f => f.FeeTypeName == "Key Replacement Fee").Id, FeeTotal = 25.00M }
            };
            _db.Fee.AddRange(fees);
            _db.SaveChanges();

            var characterNames = new (string FirstName, string LastName)[]
            {
                ("Sheldon", "Cooper"),
                ("Leonard", "Hofstadter"),
                ("Penny", "Teller"),
                ("Howard", "Wolowitz"),
                ("Raj", "Koothrappali")
            };

            for (int i = 0; i < characterNames.Length; i++)
            {
                var user = new User
                {
                    Email = $"guest{i + 1}@email.com",
                    FirstName = characterNames[i].FirstName,
                    LastName = characterNames[i].LastName,
                    Phone = $"555-000{i + 1}",
                    IsActive = true
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

                var reservation = new Reservation
                {
                    StartDate = DateTime.Today.AddDays(-i * 2),
                    EndDate = DateTime.Today.AddDays(i + 3),
                    Duration = (i + 3) + i * 2,
                    Status = i switch
                    {
                        0 => "Pending",
                        1 => "Confirmed",
                        2 => "Confirmed",
                        3 => "Active",
                        4 => "Cancelled",
                        _ => "Pending"
                    },
                    GuestId = guest.GuestID,
                    LotId = lot.Id,
                    RvId = rv.RvID,
                    OverrideReason = "None",
                    CancellationDate = i == 4 ? DateTime.Today.AddDays(-1) : null,
                    CancellationReason = i == 4 ? "Emergency" : null
                };
                _db.Reservation.Add(reservation);
                _db.SaveChanges();
            }
        }
    }
}
