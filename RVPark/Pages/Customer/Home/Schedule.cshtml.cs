using System.ComponentModel.DataAnnotations;
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

        public ScheduleModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Lot SelectedLot { get; set; } = null!;
        public Reservation Reservation { get; set; } = new();

        [BindProperty, Required(ErrorMessage = "First name is required.")]
        public string GuestFirstName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Last name is required.")]
        public string GuestLastName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Trailer length is required.")]
        public int Length { get; set; }

        [BindProperty, Range(1, 10, ErrorMessage = "Please enter at least 1 adult.")]
        public int NumberOfAdults { get; set; }

        [BindProperty, Range(0, 5, ErrorMessage = "Maximum 5 pets allowed.")]
        public int NumberOfPets { get; set; }

        [BindProperty, Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [BindProperty, Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        [BindProperty]
        public int Duration { get; set; }

        [BindProperty, Required(ErrorMessage = "License plate is required.")]
        public string? LicensePlate { get; set; }

        [BindProperty, Required(ErrorMessage = "Make is required.")]
        public string? Make { get; set; }

        [BindProperty, Required(ErrorMessage = "Model is required.")]
        public string? Model { get; set; }

        [BindProperty]
        public string? RvDescription { get; set; }

        [BindProperty]
        public string? SpecialRequests { get; set; }

        public string StatusOptions { get; } = "Pending";

        [BindProperty]
        public int LotId { get; set; }
        [BindProperty]
        public int GuestId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => l.Id == id, includes: "LotType");

            if (SelectedLot == null)
            {
                return NotFound();
            }

            LotId = SelectedLot.Id;
            StartDate = DateTime.UtcNow.Date;
            EndDate = StartDate.AddDays(1);
            Duration = 1;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claims = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var userId = claims.Value;  
            }

            return RedirectToPage("Payment", new
            {
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
                duration = Duration,
                id = LotId,
            });
        }
    }
}
