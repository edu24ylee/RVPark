using System.ComponentModel.DataAnnotations;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Customer.Home
{
    public class ScheduleModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public ScheduleModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Lot selectedLot { get; set; }

        [BindProperty, Required(ErrorMessage = "First name is required.")]
        public string guestFirstName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Last name is required.")]
        public string guestLastName { get; set; } = string.Empty;

        [BindProperty, Range(1, 10, ErrorMessage = "Please enter at least 1 adult.")]
        public int numberOfAdults { get; set; }

        [BindProperty, Range(0, 5, ErrorMessage = "Maximum 5 pets allowed.")]
        public int numberOfPets { get; set; }
        [BindProperty, Required(ErrorMessage = "Starting Date is required.")]
        public DateTime startDate { get; set; }
        [BindProperty, Required(ErrorMessage = "EndingDate is required.")]
        public DateTime endDate { get; set; }
        [BindProperty]
        public int duration { get; set; }

        [BindProperty, Required(ErrorMessage = "License Plate is required.")]
        public string? licensePlate { get; set; }
        [BindProperty, Required(ErrorMessage = "Make is required.")]
        public string? make { get; set; }
        [BindProperty, Required(ErrorMessage = "Model Plate is required.")]
        public string? model { get; set; }
        [BindProperty]
        public string? rvDescription { get; set; }
        //[BindProperty, Required(ErrorMessage = "Trailer length is required.")]
        [BindProperty, Required(ErrorMessage = "Length is required.")]
        public int length { get; set; }

        [BindProperty]
        public string? specialRequests { get; set; }

        public string statusOptions { get; } = "Pending";
        [BindProperty]
        public int lotId { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            selectedLot = await _unitOfWork.Lot.GetAsync(
                l => ((Lot)l).Id == id, includes: "LotType");
            lotId = selectedLot.Id;
            startDate = DateTime.UtcNow.Date;
            endDate = startDate.AddDays(1);
            duration = 1;

            if (selectedLot == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return RedirectToPage("Payment", new
            {
                guestFirstName,
                guestLastName,
                licensePlate,
                make,
                model,
                rvDescription,
                length,
                numberOfAdults,
                numberOfPets,
                specialRequests,
                startDate,
                endDate,
                duration,
                id = lotId
            });
        }
    }


}

