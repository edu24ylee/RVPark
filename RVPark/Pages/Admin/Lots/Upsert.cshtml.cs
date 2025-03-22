using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Data;

namespace RVPark.Pages.Admin.Lots
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        [BindProperty]
        public Lot LotObject { get; set; }
        public UpsertModel(UnitOfWork unitofWork) => _unitOfWork = unitofWork;

        public IActionResult OnGet(int? id)
        {
            LotObject = new Lot();

            if (id != 0) // edit
            {
                LotObject = _unitOfWork.Lot.Get(u => u.Id == id);
            }

            if (LotObject == null)
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
            if (LotObject.Id == 0) //if new
            {
                _unitOfWork.Lot.Add(LotObject);
            }
            else //if existing
            {
                _unitOfWork.Lot.Update(LotObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
