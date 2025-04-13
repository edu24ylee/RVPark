// Pages/Admin/FeeTypes/Index.cshtml.cs
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.FeeTypes
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<FeeType> FeeTypeList { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allTypes = await _unitOfWork.FeeType.GetAllAsync();
            FeeTypeList = allTypes.ToList();
        }
    }
}
