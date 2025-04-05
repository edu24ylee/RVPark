using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Policies
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        // The Policy object bound to form fields on the Razor Page
        [BindProperty]
        public Policy PolicyObject { get; set; }

        // Inject the UnitOfWork to access the Policy repository
        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Called when navigating to the form (GET request)
        public void OnGet(int? id)
        {
            if (id != null)
            {
                // If an ID is passed, attempt to fetch the existing policy
                PolicyObject = _unitOfWork.Policy.Get(p => p.Id == id);
            }

            // If no existing policy was found (or no ID given), initialize a new one
            if (PolicyObject == null)
            {
                PolicyObject = new Policy();
            }
        }

        // Called when the form is submitted (POST request)
        public IActionResult OnPost()
        {
            // If form validation fails, redisplay the form with errors
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // If ID = 0, this is a new policy → create
            if (PolicyObject.Id == 0)
            {
                _unitOfWork.Policy.Add(PolicyObject);
            }
            else
            {
                // Otherwise, update the existing policy
                _unitOfWork.Policy.Update(PolicyObject);
            }

            // Commit the changes to the database
            _unitOfWork.Commit();

            // Redirect to the Index page after successful save
            return RedirectToPage("./Index");
        }
    }
}
