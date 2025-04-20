using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.Fees
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Fee FeeObject { get; set; } = new();

        public List<SelectListItem> FeeTypeList { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var feeTypes = await _unitOfWork.FeeType.GetAllAsync(f => !f.IsArchived);

            FeeTypeList = feeTypes
                .Select(f => new SelectListItem
                {
                    Text = f.FeeTypeName,
                    Value = f.Id.ToString()
                }).ToList();

            if (id == null || id == 0)
            {
                FeeObject = new Fee();
            }
            else
            {
                FeeObject = await _unitOfWork.Fee.GetAsync(f => f.Id == id);
                if (FeeObject == null)
                    return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var feeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == FeeObject.FeeTypeId);
            if (feeType == null)
            {
                ModelState.AddModelError("FeeObject.FeeTypeId", "Invalid fee type.");
                return Page();
            }

            FeeObject.TriggerType = feeType.TriggerType;

            if (!ModelState.IsValid)
                return Page();

            if (FeeObject.Id == 0)
                _unitOfWork.Fee.Add(FeeObject);
            else
                _unitOfWork.Fee.Update(FeeObject);

            await _unitOfWork.CommitAsync();
            return RedirectToPage("Index");
        }
    }
}
