using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace RVPark.Pages.Admin.LotTypes
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public LotType LotTypeObject { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, int? parkId)
        {
            if (id == null || id == 0)
            {
                if (parkId == null)
                    return NotFound();

                var parkExists = await _unitOfWork.Park.GetAsync(p => p.Id == parkId);
                if (parkExists == null)
                    return NotFound();

                var start = DateTime.Today;
                LotTypeObject = new LotType
                {
                    ParkId = parkId.Value,
                    StartDate = start,
                    EndDate = start.AddYears(1)
                };
            }
            else
            {
                var lotTypeFromDb = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
                if (lotTypeFromDb == null)
                    return NotFound();

                LotTypeObject = lotTypeFromDb;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var parkExists = await _unitOfWork.Park.GetAsync(p => p.Id == LotTypeObject.ParkId);
            if (parkExists == null)
            {
                ModelState.AddModelError(string.Empty, "Selected Park does not exist.");
                return Page();
            }

            if (LotTypeObject.Id == 0)
            {
                LotTypeObject.StartDate = DateTime.Today;
                LotTypeObject.EndDate = LotTypeObject.StartDate.AddYears(1);
                _unitOfWork.LotType.Add(LotTypeObject);
            }
            else
            {
                var existing = await _unitOfWork.LotType.GetAsync(l => l.Id == LotTypeObject.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = LotTypeObject.Name;
                existing.Rate = LotTypeObject.Rate;
                existing.IsArchived = LotTypeObject.IsArchived;

                if (existing.EndDate != LotTypeObject.EndDate)
                {
                    existing.StartDate = DateTime.Today;
                    existing.EndDate = existing.StartDate.AddYears(1);
                }

                _unitOfWork.LotType.Update(existing);
                LotTypeObject = existing;
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index", new { SelectedParkId = LotTypeObject.ParkId });
        }
    }
}
