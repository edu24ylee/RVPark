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

            var lotType = new LotType
            {
                Name = "Standard",
                Rate = 40.00,
                ParkId = park.Id
            };

            _db.LotType.Add(lotType);
            _db.SaveChanges();

            _db.Lot.Add(new Lot
            {
                LotTypeId = lotType.Id,
                Description = "Pull-through lot near the entrance",
                Length = 35,
                Width = 20,
                HeightLimit = 14,
                Location = "A1",
                IsAvailable = true
            });

            _db.SaveChanges();

            var feeType = new FeeType
            {
                FeeTypeName = "Late Check-Out Fee"
            };

            _db.FeeType.Add(feeType);
            _db.SaveChanges();


            var fee = new Fee
            {
                FeeTypeId = feeType.Id,
                FeeTotal = 25.00M
            };

            _db.Fee.Add(fee);
            _db.SaveChanges();
        }
    }
}
