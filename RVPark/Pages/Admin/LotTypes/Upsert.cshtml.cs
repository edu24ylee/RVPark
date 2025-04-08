using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.LotTypes
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // The lot type being created or edited
        [BindProperty]
        public LotType LotTypeObject { get; set; } = new();

        // Loads form for creating or editing a lot type
        public async Task<IActionResult> OnGetAsync(int? id, int? parkId)
        {
            if (id == null || id == 0)
            {
                // New lot type — requires a parkId context
                if (parkId == null) return NotFound();

                LotTypeObject = new LotType
                {
                    ParkId = parkId.Value
                };
            }
            else
            {
                var lotTypeFromDb = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
                if (lotTypeFromDb == null) return NotFound();

                LotTypeObject = lotTypeFromDb;
            }

            return Page();
        }

        // Submits changes to the lot type
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (LotTypeObject.Id == 0)
            {
                _unitOfWork.LotType.Add(LotTypeObject);
            }
            else
            {
                _unitOfWork.LotType.Update(LotTypeObject);
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index", new { SelectedParkId = LotTypeObject.ParkId });
        }
    }
}
