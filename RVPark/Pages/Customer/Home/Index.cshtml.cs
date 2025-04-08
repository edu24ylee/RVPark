
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Customer.Home
{
    public class IndexModel : PageModel
    {

            private readonly UnitOfWork _unitOfWork;

            public IndexModel(UnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            public List<Lot> AvailableLots { get; set; } = new List<Lot>();

            public async Task OnGetAsync()
            {
                var lots = await _unitOfWork.Lot.GetAllAsync(l => l.IsAvailable);
                AvailableLots = lots.ToList();
            }
        }
    
}