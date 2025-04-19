using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            => _unitOfWork = unitOfWork;

        public List<LotType> AllLotTypes { get; set; } = new();
        public Lot? FeaturedLot { get; set; }
        public List<Lot> AvailableLots { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterEndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? LotTypeId { get; set; }

        public async Task OnGetAsync()
        {
            AllLotTypes = (await _unitOfWork.LotType.GetAllAsync())
                           .OrderBy(lt => lt.Name)
                           .ToList();

            var allLots = (await _unitOfWork.Lot.GetAllAsync(
                             l => !l.IsArchived && l.IsAvailable,
                             includes: "LotType"))
                           .ToList();

            if (FilterStartDate.HasValue && FilterEndDate.HasValue
                && FilterStartDate <= FilterEndDate)
            {
                var start = FilterStartDate.Value.Date;
                var end = FilterEndDate.Value.Date;
                var reservedIds = (await _unitOfWork.Reservation.GetAllAsync(
                                     r => !(r.EndDate < start || r.StartDate > end)))
                                  .Select(r => r.LotId)
                                  .ToHashSet();
                allLots = allLots.Where(l => !reservedIds.Contains(l.Id)).ToList();
            }

            if (LotTypeId.HasValue)
                allLots = allLots.Where(l => l.LotTypeId == LotTypeId).ToList();

            FeaturedLot = allLots.FirstOrDefault(l => l.IsFeatured)
                          ?? allLots.OrderByDescending(l => l.LotType?.Rate).FirstOrDefault();

            AvailableLots = allLots
                .Where(l => l.Id != FeaturedLot?.Id)
                .OrderBy(l => l.LotType?.Name)
                .ToList();
        }
    }
}
