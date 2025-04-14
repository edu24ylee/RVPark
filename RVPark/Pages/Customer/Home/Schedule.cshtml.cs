using System.ComponentModel.DataAnnotations;
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
        public decimal Length { get; set; }

        [BindProperty, Range(1, 10, ErrorMessage = "Please enter at least 1 adult.")]
        public int NumberOfAdults { get; set; }

        [BindProperty, Range(0, 5, ErrorMessage = "Maximum 5 pets allowed.")]
        public int NumberOfPets { get; set; }

        [BindProperty]
        public string? SpecialRequests { get; set; }

        public List<string> StatusOptions { get; } = new()
        {
            "Active", "Cancelled", "Confirmed", "Completed", "Pending"
        };

        public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => ((Lot)l).Id == id,
                includes: "LotType");

            if (SelectedLot == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
