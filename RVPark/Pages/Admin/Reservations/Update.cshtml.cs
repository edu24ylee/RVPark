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

        // ViewModel holds reservation info, guest name, RV, available lots/types, and original total
        [BindProperty]
        public ReservationUpdateModel ViewModel { get; set; }

        // GET: Load reservation data for update form
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Fetch reservation with guest info, RV, and assigned lot (including its type)
            var reservation = await _unitOfWork.Reservation.GetAsync(
                predicate: r => r.ReservationId == id,
                includes: "Guest.User,Rv,Lot.LotType"
            );

            if (reservation == null)
                return NotFound(); // Return 404 if reservation doesn't exist

            // Build guest's full name for display
            var guestName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}";

            // Extract the RV
            var rv = reservation.Rv;

            // Fetch all lot types for dropdown population
            var lotTypes = await _unitOfWork.LotType.GetAllAsync();

            // Get all lots that are available or currently assigned to this reservation
            var availableLots = await _unitOfWork.Lot.GetAllAsync(
                predicate: l => l.IsAvailable || l.Id == reservation.LotId,
                includes: "LotType"
            );

            // Construct ViewModel to send to Razor Page
            ViewModel = new ReservationUpdateModel
            {
                Reservation = reservation,
                Rv = rv,
                GuestName = guestName,
                LotTypes = lotTypes.ToList(),
                AvailableLots = availableLots.ToList(),
                OriginalTotal = reservation.CalculateTotal((decimal)reservation.Lot.LotType.Rate)
            };

            return Page(); // Render page with ViewModel data
        }

        // POST: Process updates to the reservation
        public async Task<IActionResult> OnPostAsync()
        {
            // Fetch current reservation and associated lot/lot type
            var res = await _unitOfWork.Reservation.GetAsync(
                predicate: r => r.ReservationId == ViewModel.Reservation.ReservationId,
                includes: "Lot.LotType"
            );

            if (res == null)
                return NotFound(); // If reservation was deleted between load and submit

            // Track the currently assigned lot
            var oldLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);

            // Update fields from the posted form
            res.StartDate = ViewModel.Reservation.StartDate;
            res.EndDate = ViewModel.Reservation.EndDate;
            res.LotId = ViewModel.Reservation.LotId;
            res.Status = ViewModel.Reservation.Status;
            res.OverrideReason = ViewModel.Reservation.OverrideReason;
            res.CancellationReason = ViewModel.Reservation.CancellationReason;

            // Update duration based on date changes
            var newDuration = (res.EndDate - res.StartDate).Days;
            res.UpdateDuration(newDuration);

            if (res.Status == "Cancelled")
            {
                // If cancelled, mark as such and free up the old lot
                res.CancelReservation();

                if (oldLot != null)
                {
                    oldLot.IsAvailable = true;
                    _unitOfWork.Lot.Update(oldLot);
                }
            }
            else
            {
                // Otherwise, reserve the newly selected lot
                var newLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);
                if (newLot != null)
                {
                    newLot.IsAvailable = false;
                    _unitOfWork.Lot.Update(newLot);
                }

                // If lot changed, make the old lot available again
                if (oldLot != null && oldLot.Id != res.LotId)
                {
                    oldLot.IsAvailable = true;
                    _unitOfWork.Lot.Update(oldLot);
                }
            }

            // Persist reservation updates to database
            _unitOfWork.Reservation.Update(res);
            await _unitOfWork.CommitAsync();

            return RedirectToPage("./Index"); // Return to reservation list
        }
    }
}
