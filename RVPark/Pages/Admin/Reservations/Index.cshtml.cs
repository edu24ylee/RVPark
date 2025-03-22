using Microsoft.AspNetCore.Mvc.RazorPages;
using ApplicationCore.Models;
using Infrastructure.Data;

namespace RVPark.Pages.Admin.Reservations
{
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
