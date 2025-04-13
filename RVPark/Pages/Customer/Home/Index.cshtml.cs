using ApplicationCore.Models;
using Infrastructure.Data;
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

        public Lot? FeaturedLot { get; set; }

        public async Task OnGetAsync()
        {
            var lots = await _unitOfWork.Lot.GetAllAsync(
                l => l.IsAvailable,
                includes: "LotType"
            );

            FeaturedLot = lots.FirstOrDefault(l => l.IsFeatured);
            FeaturedLot = lots.FirstOrDefault(l => l.IsFeatured)
           ?? lots.Where(l => l.LotType != null)
                  .OrderByDescending(l => l.LotType!.Rate)
                  .FirstOrDefault();



            AvailableLots = lots
                .Where(l => l.Id != FeaturedLot?.Id)
                .ToList();
        }
    }
}
