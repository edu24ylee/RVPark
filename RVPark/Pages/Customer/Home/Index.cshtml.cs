using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
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

        public List<Lot> AvailableLots { get; set; } = new();
        public Lot? FeaturedLot { get; set; }
        public List<LotType> AllLotTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterEndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? LotTypeId { get; set; }

        public int? SelectedLotTypeId => LotTypeId;

        public async Task OnGetAsync()
        {
            AllLotTypes = (await _unitOfWork.LotType.GetAllAsync()).ToList();

            var allLots = await _unitOfWork.Lot.GetAllAsync(
                l => !l.IsArchived && l.IsAvailable,
                includes: "LotType");

            if (FilterStartDate.HasValue && FilterEndDate.HasValue)
            {
                var reservedLotIds = (await _unitOfWork.Reservation.GetAllAsync(
                    r => !(r.EndDate < FilterStartDate || r.StartDate > FilterEndDate)))
                    .Select(r => r.LotId)
                    .ToHashSet();

                allLots = allLots.Where(l => !reservedLotIds.Contains(l.Id)).ToList();
            }

            if (LotTypeId.HasValue)
                allLots = allLots.Where(l => l.LotTypeId == LotTypeId).ToList();

            FeaturedLot = allLots.FirstOrDefault(l => l.IsFeatured)
                          ?? allLots.OrderByDescending(l => l.LotType?.Rate).FirstOrDefault();

            AvailableLots = allLots
                .Where(l => l.Id != FeaturedLot?.Id)
                .ToList();
        }
    }
}
