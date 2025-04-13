using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Policies
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public Policy PolicyObject { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || id == 0)
            {
                PolicyObject = new Policy();
            }
            else
            {
                PolicyObject = await _unitOfWork.Policy.GetAsync(p => p.Id == id.Value);
                if (PolicyObject == null) return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            if (PolicyObject.Id == 0)
                _unitOfWork.Policy.Add(PolicyObject);
            else
                _unitOfWork.Policy.Update(PolicyObject);

            return RedirectToPage("./Index");
        }
    }
}
