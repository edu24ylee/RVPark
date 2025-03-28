using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.FeeTypes
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        [BindProperty]
        public FeeType FeeTypeObject { get; set; }

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet(int? id)
        {
            if (id != null)
            {
                FeeTypeObject = _unitOfWork.FeeType.Get(ft => ft.Id == id);
            }
            if (FeeTypeObject == null)
            {
                FeeTypeObject = new FeeType();
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (FeeTypeObject.Id == 0)
            {
                _unitOfWork.FeeType.Add(FeeTypeObject);
            }
            else
            {
                _unitOfWork.FeeType.Update(FeeTypeObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
