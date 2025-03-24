using ApplicationCore.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Utilities;
using ApplicationCore.Models.ApplicationCore.Models;

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

            // Create roles
            //_roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
            //_roleManager.CreateAsync(new IdentityRole(SD.ManagerRole)).GetAwaiter().GetResult();
            //_roleManager.CreateAsync(new IdentityRole(SD.GuestRole)).GetAwaiter().GetResult();

            

            // Seed Parks
            var parks = new Park[]
            {
                new Park { Name = "Sunset Valley RV Park", Address = "123 Sunset Blvd", City = "Mesa", State = "AZ", Zipcode = "85201" },
                new Park { Name = "Mountain View Campgrounds", Address = "456 Hilltop Dr", City = "Provo", State = "UT", Zipcode = "84604" }
            };
            _db.Park.AddRange(parks);
            _db.SaveChanges();

            // Seed LotTypes
            var lotTypes = new LotType[]
            {
                new LotType { Name = "Standard", Rate = 30.00, ParkId = parks[0].Id },
                new LotType { Name = "Premium", Rate = 45.00, ParkId = parks[0].Id }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            // Seed Lots
            var lots = new Lot[]
            {
                new Lot { Location = "A1", Length = 30, Width = 10, HeightLimit = 12, IsAvailable = true, Description = "Shaded near restroom", LotTypeId = lotTypes[0].Id },
                new Lot { Location = "B2", Length = 45, Width = 12, HeightLimit = 14, IsAvailable = true, Description = "Sunny, next to office", LotTypeId = lotTypes[1].Id }
            };
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            // Seed Guests
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

            // Seed RVs
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
        }
    }
}
