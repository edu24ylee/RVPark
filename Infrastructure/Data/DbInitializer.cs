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

        /*  public void Initialize()
          {
              _db.Database.Migrate();

              if (!_db.Park.Any())
              {
                  SeedData();
              }
          }*/
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

            var policies = new List<Policy>
            {
                new Policy { PolicyName = "24-Hour Cancellation Policy", PolicyDescription = "Cancelling within 24 hours of the reservation start date will result in a penalty fee." },
                new Policy { PolicyName = "Additional Adult Fee Policy", PolicyDescription = "Each adult guest beyond 3 incurs an additional daily fee." },
                new Policy { PolicyName = "Pet Cleanup Policy", PolicyDescription = "Fee applied if pet waste is not cleaned up." }
            };
            _db.Policy.AddRange(policies);
            _db.SaveChanges();
            var feeTypes = new List<FeeType>
            {
                new FeeType { FeeTypeName = "24 Hour Cancellation Fee", Description = "Triggered if within 24 hrs", TriggerType = TriggerType.Triggered, TriggerRuleJson = "{\"HoursBefore\":24,\"PenaltyPercent\":100}" },
                new FeeType { FeeTypeName = "Extra Adults Fee", Description = "Triggered per adult over 3", TriggerType = TriggerType.Triggered, TriggerRuleJson = "{\"Threshold\":3,\"Fee\":1.0}" },
                new FeeType { FeeTypeName = "Pet Cleanup Violation", Description = "Manual fee for uncleaned pet waste", TriggerType = TriggerType.Manual }
            };
            _db.FeeType.AddRange(feeTypes);
            _db.SaveChanges();

            var lots = lotTypes.SelectMany(lt => Enumerable.Range(1, 5).Select(i => new Lot
            {
                LotTypeId = lt.Id,
                Location = $"{lt.Name[0]}-{i}",
                Width = 20,
                Length = 35,
                IsAvailable = true
            })).ToList();
            _db.Lot.AddRange(lots);
            _db.SaveChanges();

            var guestInfos = new List<(string First, string Last)>
            {
                ("Sheldon", "Cooper"),
                ("Leonard", "Hofstadter"),
                ("Penny", "Teller"),
                ("Howard", "Wolowitz"),
                ("Raj", "Koothrappali")
            };

            var guestList = new List<Guest>();
            var rvList = new List<RV>();

            for (int i = 0; i < guestInfos.Count; i++)
            {
                var (first, last) = guestInfos[i];
                var email = $"guest{last.ToLower()}@rv.com";
                var identityUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

                var identityResult = _userManager.CreateAsync(identityUser, "Guest123!").GetAwaiter().GetResult();
                if (identityResult.Succeeded)
                {
                    _userManager.AddToRoleAsync(identityUser, SD.GuestRole).GetAwaiter().GetResult();
                }

                var user = new User
                {
                    FirstName = first,
                    LastName = last,
                    Email = email,
                    Phone = "555-0101",
                    IdentityUserId = identityUser.Id,
                    IsActive = true
                };
                _db.User.Add(user);
                _db.SaveChanges();

                var guest = new Guest
                {
                    UserID = user.UserID,
                    DodId = 3000 + i
                };
                _db.Guest.Add(guest);
                _db.SaveChanges();
                guestList.Add(guest);

                var rv = new RV
                {
                    GuestID = guest.GuestID,
                    LicensePlate = $"RV-{i + 1:000}",
                    Make = "Winnebago",
                    Model = $"Model-{i + 1}",
                    Description = $"Test RV {i + 1}",
                    Length = 25 + i
                };
                _db.RV.Add(rv);
                _db.SaveChanges();
                rvList.Add(rv);
            }

            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    GuestId = guestList[0].GuestID,
                    RvId = rvList[0].RvID,
                    LotId = lots[0].Id,
                    StartDate = today.AddDays(2),
                    EndDate = today.AddDays(6),
                    Duration = 4,
                    Status = SD.StatusPending,
                    NumberOfAdults = 2,
                    NumberOfPets = 1
                },
                new Reservation
                {
                    GuestId = guestList[1].GuestID,
                    RvId = rvList[1].RvID,
                    LotId = lots[1].Id,
                    StartDate = today.AddDays(3),
                    EndDate = today.AddDays(6),
                    Duration = 3,
                    Status = SD.StatusActive,
                    NumberOfAdults = 4,
                    NumberOfPets = 0,
                    OverrideReason = "Admin-approved exception: elderly family"
                },
                new Reservation
                {
                    GuestId = guestList[2].GuestID,
                    RvId = rvList[2].RvID,
                    LotId = lots[2].Id,
                    StartDate = today.AddDays(7),
                    EndDate = today.AddDays(12),
                    Duration = 5,
                    Status = SD.StatusConfirmed,
                    NumberOfAdults = 3,
                    NumberOfPets = 2
                },
                new Reservation
                {
                    GuestId = guestList[3].GuestID,
                    RvId = rvList[3].RvID,
                    LotId = lots[3].Id,
                    StartDate = today.AddDays(-5),
                    EndDate = today.AddDays(-1),
                    Duration = 4,
                    Status = SD.StatusCompleted,
                    NumberOfAdults = 3,
                    NumberOfPets = 0
                },
                new Reservation
                {
                    GuestId = guestList[4].GuestID,
                    RvId = rvList[4].RvID,
                    LotId = lots[4].Id,
                    StartDate = today.AddDays(1),
                    EndDate = today.AddDays(4),
                    Duration = 3,
                    Status = SD.StatusCancelled,
                    NumberOfAdults = 5,
                    NumberOfPets = 1,
                    CancellationReason = "Medical emergency",
                    OverrideReason = "Fee waived by admin due to proof provided"
                }
            };

            _db.Reservation.AddRange(reservations);
            _db.SaveChanges();


            var cleanupPolicy = _db.Policy.FirstOrDefault(p => p.PolicyName.Contains("Cleanup"));
            if (cleanupPolicy != null)
            {
                var fee = new Fee
                {
                    FeeTypeId = feeTypes.First().Id,
                    TriggeringPolicyId = cleanupPolicy.Id,
                    FeeTotal = 25.0M,
                    AppliedDate = DateTime.UtcNow,
                    Notes = "Auto-triggered on creation",
                    ReservationId = reservations[0].ReservationId,
                    TriggerType = TriggerType.Triggered
                };
                _db.Fee.Add(fee);
                _db.SaveChanges();
            }
            var triggeredPolicy = _db.Policy.FirstOrDefault(p => p.PolicyName.Contains("Pet Cleanup"));
            var feeType = _db.FeeType.FirstOrDefault(f => f.FeeTypeName.Contains("Pet Cleanup"));

            if (triggeredPolicy != null && feeType != null)
            {
                var cleanupFee = new Fee
                {
                    FeeTypeId = feeType.Id,
                    TriggeringPolicyId = triggeredPolicy.Id,
                    FeeTotal = 25.00M,
                    AppliedDate = DateTime.UtcNow,
                    Notes = "Pet cleanup violation",
                    ReservationId = reservations[0].ReservationId,
                    TriggerType = TriggerType.Triggered
                };

                _db.Fee.Add(cleanupFee);
                _db.SaveChanges();
            }


        }
    }
}
