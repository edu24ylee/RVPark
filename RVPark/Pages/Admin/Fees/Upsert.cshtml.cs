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
        public List<SelectListItem> PolicyList { get; set; } = new();

        public List<(int FeeTypeId, TriggerType TriggerType)> FeeTypeTriggerTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var feeTypes = await _unitOfWork.FeeType.GetAllAsync();
            var policies = await _unitOfWork.Policy.GetAllAsync();

            FeeTypeList = feeTypes
                .Where(f => !f.IsArchived)
                .Select(f => new SelectListItem { Text = f.FeeTypeName, Value = f.Id.ToString() })
                .ToList();

            PolicyList = policies
                .Select(p => new SelectListItem { Text = p.PolicyName, Value = p.Id.ToString() })
                .ToList();

            FeeTypeTriggerTypes = feeTypes
                .Select(f => (f.Id, f.TriggerType))
                .ToList();

            if (id == null || id == 0)
            {
                FeeObject = new Fee();
            }
            else
            {
                FeeObject = await _unitOfWork.Fee.GetAsync(f => f.Id == id.Value, includes: "FeeType,TriggeringPolicy");
                if (FeeObject == null) return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(FeeObject.Id);
                return Page();
            }

            if (FeeObject.Id == 0)
            {
                _unitOfWork.Fee.Add(FeeObject);
            }
            else
            {
                _unitOfWork.Fee.Update(FeeObject);
            }

            return RedirectToPage("Index");
        }
    }
}
