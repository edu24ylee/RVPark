using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.FeeTypes
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public FeeType FeeTypeObject { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || id == 0)
            {
                FeeTypeObject = new FeeType();
            }
            else
            {
                FeeTypeObject = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
                if (FeeTypeObject == null)
                    return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (FeeTypeObject.Id == 0)
                _unitOfWork.FeeType.Add(FeeTypeObject);
            else
                _unitOfWork.FeeType.Update(FeeTypeObject);

            await _unitOfWork.CommitAsync();
            return RedirectToPage("Index");
        }
    }
}
