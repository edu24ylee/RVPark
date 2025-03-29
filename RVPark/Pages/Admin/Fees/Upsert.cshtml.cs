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

        [BindProperty]
        public Fee FeeObject { get; set; }

        public IEnumerable<SelectListItem> FeeTypeList { get; set; }
        public IEnumerable<SelectListItem> PolicyList { get; set; }

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet(int? id)
        {
            if(id != null)
            {
                FeeObject = _unitOfWork.Fee.Get(f => f.Id == id);
            }

            if(FeeObject == null)
            {
                FeeObject = new Fee();
            }

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

        public IActionResult OnPost()
        {
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
            if (FeeObject.Id == 0)
            {
                _unitOfWork.Fee.Add(FeeObject);
            }
            else
            {
                _unitOfWork.Fee.Update(FeeObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
