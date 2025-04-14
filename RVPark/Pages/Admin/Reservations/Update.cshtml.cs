// UpdateModel.cs (backend logic)
using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace RVPark.Pages.Admin.Reservations
{
    public class UpdateModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpdateModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty] public ReservationUpdateModel ViewModel { get; set; } = null!;
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Page("/Admin/Reservations/Update", new { id });

            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == id,
                includes: "Guest.User,Rv,Lot.LotType");

            if (reservation == null) return NotFound();

            var lotTypes = await _unitOfWork.LotType.GetAllAsync();
            var availableLots = await _unitOfWork.Lot.GetAllAsync(
                l => l.IsAvailable || l.Id == reservation.LotId,
                includes: "LotType");

            var manualFees = await _unitOfWork.FeeType.GetAllAsync(f => f.TriggerType == TriggerType.Manual && !f.IsArchived);

            reservation.Duration = (reservation.EndDate - reservation.StartDate).Days;

            ViewModel = new ReservationUpdateModel
            {
                Reservation = reservation,
                GuestName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}",
                Rv = reservation.Rv,
                LotTypes = lotTypes.ToList(),
                AvailableLots = availableLots.ToList(),
                OriginalTotal = reservation.CalculateTotal((decimal)(reservation.Lot?.LotType?.Rate ?? 0)),
                ManualFeeOptions = manualFees.ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var res = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == ViewModel.Reservation.ReservationId,
                includes: "Lot.LotType");

            if (res == null) return NotFound();

            var oldLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);

            res.StartDate = ViewModel.Reservation.StartDate;
            res.EndDate = ViewModel.Reservation.EndDate;
            res.LotId = ViewModel.Reservation.LotId;
            res.OverrideReason = ViewModel.Reservation.OverrideReason;
            res.CancellationReason = ViewModel.Reservation.CancellationReason;
            res.Duration = Math.Max(0, (res.EndDate - res.StartDate).Days);

            if (res.Status == "Cancelled")
            {
                res.CancelReservation();
                if (oldLot != null) oldLot.IsAvailable = true;
                if (oldLot != null) _unitOfWork.Lot.Update(oldLot);
            }
            else
            {
                var newLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);
                if (newLot != null) newLot.IsAvailable = false;
                if (newLot != null) _unitOfWork.Lot.Update(newLot);
                if (oldLot != null && oldLot.Id != res.LotId)
                {
                    oldLot.IsAvailable = true;
                    _unitOfWork.Lot.Update(oldLot);
                }
            }

            var user = User.FindFirstValue(ClaimTypes.Role);
            if ((user == SD.AdminRole || user == SD.CampHostRole || user == SD.SuperAdminRole) &&
                ViewModel.ManualFeeTypeId.HasValue)
            {
                var manualFeeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == ViewModel.ManualFeeTypeId);
                if (manualFeeType != null)
                {
                    _unitOfWork.Fee.Add(new Fee
                    {
                        FeeTypeId = manualFeeType.Id,
                        FeeTotal = 0,
                        ReservationId = res.ReservationId,
                        Notes = $"Manually added fee: {manualFeeType.FeeTypeName}"
                    });
                }
            }

            _unitOfWork.Reservation.Update(res);
            await _unitOfWork.CommitAsync();

            return !string.IsNullOrEmpty(ReturnUrl) ? Redirect(ReturnUrl) : RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetAvailableLotsAsync(int lotTypeId, int trailerLength, DateTime startDate, DateTime endDate)
        {
            var existingReservations = await _unitOfWork.Reservation.GetAllAsync(
                r => !(r.EndDate < startDate || r.StartDate > endDate));

            var reservedLotIds = existingReservations.Select(r => r.LotId).Distinct();

            var lots = await _unitOfWork.Lot.GetAllAsync(
                l => l.LotTypeId == lotTypeId &&
                     l.Length >= trailerLength &&
                     !reservedLotIds.Contains(l.Id) &&
                     l.IsAvailable && !l.IsArchived,
                includes: "LotType");

            var result = lots.Select(l => new
            {
                id = l.Id,
                location = l.Location,
                description = l.Description,
                featuredImageUrl = l.FeaturedImage,
                lotTypeRate = l.LotType?.Rate ?? 0
            });

            return new JsonResult(result);
        }
    }
}
