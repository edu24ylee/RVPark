using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace RVPark.Pages.Customer.Home
{
    public class DetailsModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public DetailsModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Lot SelectedLot { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => l.Id == id,
                includes: "LotType");

            if (SelectedLot == null)
                return NotFound();

            return Page();
        }

    }
}
