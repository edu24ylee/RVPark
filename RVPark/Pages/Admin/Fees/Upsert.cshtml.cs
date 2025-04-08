using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.Fees
{
    // Handles both creation and editing of Fee records
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        // This model is bound to form fields on the page
        [BindProperty]
        public Fee FeeObject { get; set; }

        // Dropdown list of available FeeTypes
        public IEnumerable<SelectListItem> FeeTypeList { get; set; }

        // Dropdown list of available Policies
        public IEnumerable<SelectListItem> PolicyList { get; set; }

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Called when navigating to the form
        public void OnGet(int? id)
        {
            // If editing, load existing fee
            if (id != null)
            {
                FeeObject = _unitOfWork.Fee.Get(f => f.Id == id);
            }

            // If no fee found (new record), initialize an empty one
            if (FeeObject == null)
            {
                FeeObject = new Fee();
            }

            // Populate dropdowns
            var feeTypes = _unitOfWork.FeeType.GetAll();
            FeeTypeList = feeTypes.Select(ft => new SelectListItem
            {
                Text = ft.FeeTypeName,
                Value = ft.Id.ToString()
            });

            var policies = _unitOfWork.Policy.GetAll();
            PolicyList = policies.Select(p => new SelectListItem
            {
                Text = p.PolicyName,
                Value = p.Id.ToString()
            });
        }

        // Called when submitting the form
        public IActionResult OnPost()
        {
            // Re-populate dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                var feeTypes = _unitOfWork.FeeType.GetAll();
                FeeTypeList = feeTypes.Select(ft => new SelectListItem
                {
                    Text = ft.FeeTypeName,
                    Value = ft.Id.ToString()
                });

                var policies = _unitOfWork.Policy.GetAll();
                PolicyList = policies.Select(p => new SelectListItem
                {
                    Text = p.PolicyName,
                    Value = p.Id.ToString()
                });

                return Page();
            }

            // Insert or update based on whether an ID exists
            if (FeeObject.Id == 0)
            {
                _unitOfWork.Fee.Add(FeeObject);
            }
            else
            {
                _unitOfWork.Fee.Update(FeeObject);
            }

            // Save changes and redirect to index
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
