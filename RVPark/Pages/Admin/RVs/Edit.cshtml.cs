using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.RVs
{
    public class EditModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public EditModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Rv Rv { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Rv = await _unitOfWork.Rv.GetAsync(r => r.RvId == id, includes: "Guest");
            if (Rv == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _unitOfWork.Rv.GetAsync(r => r.RvId == Rv.RvId);
            if (existing == null)
                return NotFound();

            existing.Make = Rv.Make;
            existing.Model = Rv.Model;
            existing.LicensePlate = Rv.LicensePlate;
            existing.Length = Rv.Length;
            existing.Description = Rv.Description;

            _unitOfWork.Rv.Update(existing);
            await _unitOfWork.CommitAsync();

            StatusMessage = "RV updated successfully.";

            if (!string.IsNullOrWhiteSpace(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("/Admin/RVs/Index");
        }
    }
}
