using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Lots
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Park selected in the dropdown (passed via query string)
        [BindProperty(SupportsGet = true)]
        public int? SelectedParkId { get; set; }

        // Dropdown list of all available parks
        public List<SelectListItem> ParkList { get; set; } = new();

        // Loads all parks for filtering on page load
        public async Task OnGetAsync()
        {
            var parks = await _unitOfWork.Park.GetAllAsync();
            ParkList = parks.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();
        }
    }
}
