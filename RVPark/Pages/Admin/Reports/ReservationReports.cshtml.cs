using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Reports
{
    public class ReservationReports : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public ReservationReports(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        public List<Reservation> Reservations { get; set; } = new();

        public bool ReportGenerated { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (StartDate == default || EndDate == default || StartDate > EndDate)
            {
                ReportGenerated = false;
                return Page();
            }

            Reservations = (await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= StartDate && r.EndDate <= EndDate,
                includes: "Guest.User,Lot"
            )).OrderBy(r => r.Status).ToList();

            ReportGenerated = true;
            return Page();
        }
    }
}
