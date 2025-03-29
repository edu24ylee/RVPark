using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Policies
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        [BindProperty]
        public Policy PolicyObject { get; set; }

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet(int? id)
        {
            if (id != null)
            {
                PolicyObject = _unitOfWork.Policy.Get(p => p.Id == id);
            }
            if (PolicyObject == null)
            {
                PolicyObject = new Policy();
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (PolicyObject.Id == 0)
            {
                _unitOfWork.Policy.Add(PolicyObject);
            }
            else
            {
                _unitOfWork.Policy.Update(PolicyObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
