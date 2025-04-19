using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Customer.Home
{
    [Authorize]
    public class ScheduleModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public ScheduleModel(UnitOfWork unitOfWork) =>
            _unitOfWork = unitOfWork;

        public Lot SelectedLot { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public string GuestFirstName { get; set; } = string.Empty;

        [BindProperty]
        public string GuestLastName { get; set; } = string.Empty;

        [BindProperty]
        public int Length { get; set; }

        [BindProperty]
        public string LicensePlate { get; set; } = string.Empty;

        [BindProperty]
        public string Make { get; set; } = string.Empty;

        [BindProperty]
        public string Model { get; set; } = string.Empty;

        [BindProperty]
        public string RvDescription { get; set; } = string.Empty;

        [BindProperty]
        public string SpecialRequests { get; set; } = string.Empty;

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        [BindProperty]
        public int Duration { get; set; }

        [BindProperty]
        public int NumberOfAdults { get; set; }

        [BindProperty]
        public int NumberOfPets { get; set; }

        [BindProperty]
        public int LotId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => l.Id == id,
                includes: "LotType");
            if (SelectedLot == null)
                return NotFound();

            LotId = id;
            StartDate = DateTime.UtcNow.Date;
            EndDate = StartDate.AddDays(1);
            Duration = 1;
            NumberOfAdults = 1;
            NumberOfPets = 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _unitOfWork.User.GetAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
                return Challenge();

            var guest = await _unitOfWork.Guest.GetAsync(g => g.UserId == appUser.UserId);
            if (guest == null)
            {
                guest = new Guest { UserId = appUser.UserId, DodId = 0 };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();
            }

            return RedirectToPage("Payment", new
            {
                id = LotId,
                guestFirstName = GuestFirstName,
                guestLastName = GuestLastName,
                licensePlate = LicensePlate,
                make = Make,
                model = Model,
                rvDescription = RvDescription,
                length = Length,
                numberOfAdults = NumberOfAdults,
                numberOfPets = NumberOfPets,
                specialRequests = SpecialRequests,
                startDate = StartDate,
                endDate = EndDate,
                duration = Duration
            });
        }
    }
}
