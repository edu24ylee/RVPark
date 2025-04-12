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

       //[BindProperty, Required(ErrorMessage = "First name is required.")]
       public string guestFirstName { get; set; } = string.Empty;

       //[BindProperty, Required(ErrorMessage = "Last name is required.")]
       public string guestLastName { get; set; } = string.Empty;

       //[BindProperty, Required(ErrorMessage = "Trailer length is required.")]
        public decimal length { get; set; }

        //[Range(1, 10, ErrorMessage = "Please enter at least 1 adult.")]
        public int numberOfAdults { get; set; }

       // [Range(0, 5, ErrorMessage = "Maximum 5 pets allowed.")]
        public int numberOfPets { get; set; }
        //[BindProperty, Required(ErrorMessage = "Starting Date is required.")]
        public DateTime startDate { get; set; }
        //[BindProperty, Required(ErrorMessage = "EndingDate is required.")]
        public DateTime endDate { get; set; }
        public int duration { get; set; }

        public string? specialRequests { get; set; }

        public List<string> statusOptions { get; } = new()
        {
            "Active", "Cancelled", "Confirmed", "Completed", "Pending"
        };

        public async Task<IActionResult> OnGetAsync(int id)
            {
                selectedLot = await _unitOfWork.Lot.GetAsync(
                    l => ((Lot)l).Id == id, includes: "LotType");

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
            TempData["FirstName"] = guestFirstName;
            TempData["LastName"] = guestLastName;
            TempData["TrailerLength"] = length;
            TempData["Adults"] = numberOfAdults;
            TempData["Pets"] = numberOfPets;
            TempData["SpecialRequests"] = specialRequests;
            TempData["StartDate"] = startDate;
            TempData["SpecialRequests"] = endDate;
            TempData["startDate"] = duration;
            TempData["selectedLot"] = selectedLot;

            return RedirectToPage("Payment");
        }

    }
}
