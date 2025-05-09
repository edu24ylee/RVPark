using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Reservations
{
    public class PaymentModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public PaymentModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty(SupportsGet = true)]
        public int ReservationId { get; set; }

        public string GuestFullName { get; set; } = string.Empty;
        public decimal TotalDue { get; set; }

        [BindProperty]
        public decimal AmountPaid { get; set; }

        [BindProperty]
        public string Notes { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int reservationId)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == reservationId,
                includes: "Guest.User");

            if (reservation == null || reservation.Guest?.User == null)
                return NotFound();

            ReservationId = reservation.ReservationId;
            GuestFullName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}";
            TotalDue = reservation.TotalDue;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == ReservationId,
                includes: "Guest.Reservations");

            if (reservation == null || reservation.Guest == null)
                return NotFound();

            var guest = reservation.Guest;
            var outstanding = reservation.TotalDue - reservation.AmountPaid;

            if (AmountPaid <= 0 || AmountPaid > outstanding)
            {
                ModelState.AddModelError(string.Empty, $"Payment must be between $0.01 and ${outstanding:F2}");
                return await OnGetAsync(ReservationId);
            }

            reservation.AmountPaid += AmountPaid;

            guest.Balance = guest.Reservations
                .Where(r => r.Status != "Cancelled")
                .Sum(r => r.TotalDue - r.AmountPaid);

            var payment = new Payment
            {
                GuestId = guest.GuestId,
                ReservationId = reservation.ReservationId,
                Amount = AmountPaid,
                PaymentDate = DateTime.UtcNow,
                Notes = Notes,
                Method = "Manual",
                RecordedBy = User.Identity?.Name ?? "System"
            };

            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Reservation.Update(reservation);
            _unitOfWork.Guest.Update(guest);
            await _unitOfWork.CommitAsync();

            TempData["Success"] = $"Payment of {AmountPaid:C} applied to Reservation #{ReservationId}.";
            return RedirectToPage("/Admin/Reservations/Index");
        }

    }
}
