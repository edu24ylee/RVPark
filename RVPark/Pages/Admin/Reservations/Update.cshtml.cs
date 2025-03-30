using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Data;

namespace RVPark.Pages.Admin.Reservations
{
    public class UpdateModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpdateModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public ReservationUpdateModel ViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(
                predicate: r => r.ReservationId == id,
                includes: "Guest.User,Rv,Lot.LotType"
            );

            if (reservation == null)
                return NotFound();

            var guestName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}";
            var rv = reservation.Rv;

            var lotTypes = await _unitOfWork.LotType.GetAllAsync();
            var availableLots = await _unitOfWork.Lot.GetAllAsync(
                predicate: l => l.IsAvailable || l.Id == reservation.LotId,
                includes: "LotType"
            );

            ViewModel = new ReservationUpdateModel
            {
                Reservation = reservation,
                Rv = rv,
                GuestName = guestName,
                LotTypes = lotTypes.ToList(),
                AvailableLots = availableLots.ToList(),
                OriginalTotal = reservation.CalculateTotal((decimal)reservation.Lot.LotType.Rate)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var res = await _unitOfWork.Reservation.GetAsync(
                predicate: r => r.ReservationId == ViewModel.Reservation.ReservationId,
                includes: "Lot.LotType"
            );

            if (res == null)
                return NotFound();

            res.StartDate = ViewModel.Reservation.StartDate;
            res.EndDate = ViewModel.Reservation.EndDate;
            res.LotId = ViewModel.Reservation.LotId;
            res.Status = ViewModel.Reservation.Status;
            res.OverrideReason = ViewModel.Reservation.OverrideReason;
            res.CancellationReason = ViewModel.Reservation.CancellationReason;

            var newDuration = (res.EndDate - res.StartDate).Days;
            res.UpdateDuration(newDuration);

            if (res.Status == "Cancelled")
                res.CancelReservation();

            _unitOfWork.Reservation.Update(res);
            await _unitOfWork.CommitAsync();

            return RedirectToPage("./Index");
        }
    }
}
