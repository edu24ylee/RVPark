using Microsoft.AspNetCore.Mvc.RazorPages;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Pages.Admin.Reservations
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Reservation> Reservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            Reservations = (List<Reservation>)await _unitOfWork.Reservation.GetAllAsync(
                includes: "Guest.User,Lot,Rv"
            );
        }
    }
}
