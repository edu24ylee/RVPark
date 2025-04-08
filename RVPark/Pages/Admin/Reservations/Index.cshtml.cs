using Microsoft.AspNetCore.Mvc.RazorPages;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Pages.Admin.Reservations
{
    // Disables response caching to ensure fresh data every time the page loads
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // This list holds all reservations pulled from the database
        public List<Reservation> Reservations { get; set; } = new();

        // Executed on GET request to this Razor Page
        public async Task OnGetAsync()
        {
            // Retrieves all reservations, including related Guest→User, Lot, and RV information
            // This eager loading ensures all related data is available for display in the view
            Reservations = (List<Reservation>)await _unitOfWork.Reservation.GetAllAsync(
                includes: "Guest.User,Lot,Rv"
            );
        }
    }
}
