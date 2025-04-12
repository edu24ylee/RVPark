using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

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
            if (id is > 0)
            {
                var fromDb = await _unitOfWork.FeeType.GetAsync(ft => ft.Id == id);
                if (fromDb == null)
                    return NotFound();

                FeeTypeObject = fromDb;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (FeeTypeObject.Id == 0)
            {
                _unitOfWork.FeeType.Add(FeeTypeObject);
            }
            else
            {
                var existing = await _unitOfWork.FeeType.GetAsync(ft => ft.Id == FeeTypeObject.Id);
                if (existing == null)
                    return NotFound();

                existing.FeeTypeName = FeeTypeObject.FeeTypeName;
                existing.Description = FeeTypeObject.Description;
                existing.TriggerType = FeeTypeObject.TriggerType;
                existing.TriggerRuleJson = FeeTypeObject.TriggerRuleJson;
                existing.IsArchived = FeeTypeObject.IsArchived;

                _unitOfWork.FeeType.Update(existing);
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index");
        }
    }
}
