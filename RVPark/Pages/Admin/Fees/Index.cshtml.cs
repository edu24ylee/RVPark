using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.Fees
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<SelectListItem> FeeTypeOptions { get; set; } = new();

        public void OnGet()
        {
            var feeTypes = _unitOfWork.FeeType.GetAll();
            FeeTypeOptions = feeTypes.Select(ft => new SelectListItem
            {
                Text = ft.FeeTypeName,
                Value = ft.Id.ToString()
            }).ToList();
        }
    }
}
