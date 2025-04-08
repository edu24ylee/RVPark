using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Parks
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        // The Park object bound to form fields in the Upsert Razor Page
        [BindProperty]
        public Park ParkObject { get; set; }

        // Inject the UnitOfWork to access repositories
        public UpsertModel(UnitOfWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }

        // Handles GET requests to load the form
        // If `id` is null → creating a new park
        // If `id` has value → editing an existing park
        public IActionResult OnGet(int? id)
        {
            ParkObject = new Park(); // Default empty object for create mode

            if (id != null && id != 0)
            {
                // Fetch existing park by ID for edit mode
                ParkObject = _unitOfWork.Park.Get(u => u.Id == id);
                if (ParkObject == null)
                {
                    return NotFound(); // Handle invalid ID
                }
            }

            return Page(); // Show the form with populated or blank data
        }

        // Handles POST requests when form is submitted
        public IActionResult OnPost()
        {
            // If any validation rules failed, redisplay the form
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // If ID is 0 → new park, else update existing one
            if (ParkObject.Id == 0)
            {
                _unitOfWork.Park.Add(ParkObject);
            }
            else
            {
                _unitOfWork.Park.Update(ParkObject);
            }

            // Save changes to the database
            _unitOfWork.Commit();

            // Redirect back to Index page after successful save
            return RedirectToPage("./Index");
        }
    }
}
