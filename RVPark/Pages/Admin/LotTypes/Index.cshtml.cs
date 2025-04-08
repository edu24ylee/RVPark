using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.LotTypes
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Used to filter lot types by a selected park
        [BindProperty(SupportsGet = true)]
        public int? SelectedParkId { get; set; }

        // List of parks for dropdown filtering
        public List<SelectListItem> ParkList { get; set; } = new();

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
