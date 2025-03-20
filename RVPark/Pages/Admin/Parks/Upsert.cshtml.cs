using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Parks
{
    public class UpsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public Park ParkObject { get; set; }
        public UpsertModel(IUnitOfWork unitofWork) => _unitOfWork = unitofWork;

        public IActionResult OnGet(int? id)
        {
            ParkObject = new Park();

            if (id != 0) // edit
            {
                ParkObject = _unitOfWork.Park.Get(u => u.Id == id);
            }

            if (ParkObject == null)
            {
                return NotFound();
            }
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (ParkObject.Id == 0) //if new
            {
                _unitOfWork.Park.Add(ParkObject);
            }
            else //if existing
            {
                _unitOfWork.Park.Update(ParkObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
