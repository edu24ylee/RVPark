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
            // Seed LotTypes
            var lotTypes = new List<LotType>
            {
                new LotType { Name = "Standard", Rate = 40.00, ParkId = park.Id },
                new LotType { Name = "Premium", Rate = 55.00, ParkId = park.Id },
                new LotType { Name = "Deluxe", Rate = 70.00, ParkId = park.Id }
            };
            _db.LotType.AddRange(lotTypes);
            _db.SaveChanges();

            // Seed Lots (15 total, 5 per LotType)
            var lots = new List<Lot>();
            int lotCounter = 1;
            foreach (var lt in lotTypes)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var lot = new Lot
                    {
                        LotTypeId = lt.Id,
                        Description = $"{lt.Name} Lot {i}",
                        Length = new[] { 30.0, 35.0, 40.0 }[new Random().Next(0, 3)],
                        Width = 20,
                        Location = $"{lt.Name[0]}{i}",
                        IsAvailable = true
                    };
                    lots.Add(lot);
                }
            }
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            // Seed Users, Guests, RVs, and Reservations with variety
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

                var user = new User
                {
                    Email = $"guest{i + 1}@email.com",
                    FirstName = first,
                    LastName = last,
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

                // Add 3 reservations per guest
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
            // Employee Users
            var employeeUsers = new List<(User user, string role)>
            {
                (new User { FirstName = "Janet", LastName = "Walker", Email = "janet@rvpark.com", Phone = "555-0100", IsActive = true }, "Manager"),
                (new User { FirstName = "Tom", LastName = "Barnes", Email = "tom@rvpark.com", Phone = "555-0101", IsActive = true }, "Maintenance"),
                (new User { FirstName = "Maria", LastName = "Gonzalez", Email = "maria@rvpark.com", Phone = "555-0102", IsActive = true }, "Guest")
            };

            foreach (var (empUser, role) in employeeUsers)
            {
                _db.User.Add(empUser);
                _db.SaveChanges();

                var emp = new Employee
                {
                    UserID = empUser.UserID,
                    Role = role
                };

                _db.Employee.Add(emp);
                _db.SaveChanges();
            }

        }
    }
}
