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

        [BindProperty]
        public Park ParkObject { get; set; }

        public UpsertModel(UnitOfWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }

        public IActionResult OnGet(int? id)
        {
            ParkObject = new Park();

            if (id != null && id != 0)
            {
                ParkObject = _unitOfWork.Park.Get(u => u.Id == id);
                if (ParkObject == null)
                {
                    return NotFound();
                }
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ParkObject.Id == 0)
            {
                _unitOfWork.Park.Add(ParkObject);
            }
            else
            {
                _unitOfWork.Park.Update(ParkObject);
            }

            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
