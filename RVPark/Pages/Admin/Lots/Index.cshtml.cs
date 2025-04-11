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

        [BindProperty(SupportsGet = true)]
        public int? SelectedParkId { get; set; }

        public List<SelectListItem> ParkList { get; set; } = new();

        public async Task OnGetAsync()
        {
            var parks = await _unitOfWork.Park.GetAllAsync();
            ParkList = parks.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            if (SelectedParkId == null)
            {
                var defaultPark = parks.FirstOrDefault(p => p.Name == "Desert Eagle Nellis AFB");
                if (defaultPark != null)
                {
                    SelectedParkId = defaultPark.Id;
                }
            }
        }
    }

}
