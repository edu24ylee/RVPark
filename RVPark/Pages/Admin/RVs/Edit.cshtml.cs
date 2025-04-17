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
        public RV Rv { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Rv = await _unitOfWork.RV.GetAsync(r => r.RvID == id, includes: "Guest");

            if (Rv == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existingRv = await _unitOfWork.RV.GetAsync(r => r.RvID == Rv.RvID);
            if (existingRv == null)
                return NotFound();

            existingRv.Make = Rv.Make;
            existingRv.Model = Rv.Model;
            existingRv.LicensePlate = Rv.LicensePlate;
            existingRv.Length = Rv.Length;
            existingRv.Description = Rv.Description;

            _unitOfWork.RV.Update(existingRv);
            await _unitOfWork.CommitAsync();

            StatusMessage = "RV updated successfully.";

            if (!string.IsNullOrWhiteSpace(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectToPage("/Admin/RVs/Index");
        }
    }
}
