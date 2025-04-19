using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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

            var manualFees = await _unitOfWork.Fee.GetAllAsync(
                f => f.TriggerType == TriggerType.Manual && !f.IsArchived && f.ReservationId == null,
                includes: "FeeType");

            var manualFeeOptions = manualFees.Select(f => new ManualFeeOptionViewModel
            {
                Id = f.Id,
                FeeTypeName = f.FeeType?.FeeTypeName ?? "Unnamed",
                FeeTotal = f.FeeTotal
            }).ToList();

            ViewModel = new ReservationUpdateModel
            {
                Reservation = reservation,
                GuestName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}",
                Rv = reservation.Rv,
                LotTypes = lotTypes.ToList(),
                AvailableLots = availableLots.ToList(),
                OriginalTotal = reservation.CalculateTotal((decimal)(reservation.Lot?.LotType?.Rate ?? 0)),
                ManualFeeOptions = manualFeeOptions
            };


            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var action = Request.Form["action"];

            var res = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == ViewModel.Reservation.ReservationId,
                includes: "Lot.LotType,Guest.Reservations");

            if (res == null)
                return NotFound();

            if (action == "confirmCancel")
            {
                // === CANCELLATION ===
                var overrideStr = Request.Form["overridePercent"];
                var reason = Request.Form["cancelOverrideReason"];
                var overrideChecked = Request.Form["cancelOverride"] == "on";
                int? overridePercent = int.TryParse(overrideStr, out var parsedPercent) ? parsedPercent : null;

                var feeType = await _unitOfWork.FeeType.GetAsync(f =>
                    f.FeeTypeName == "Cancellation Fee" && f.TriggerType == TriggerType.Triggered);

                decimal rate = (decimal)(res.Lot?.LotType?.Rate ?? 0);
                res.Duration = 1;
                decimal cancellationFee = 0m;

                var hoursBeforeStart = (res.StartDate - DateTime.UtcNow).TotalHours;
                bool within24Hours = hoursBeforeStart <= 24;

                if (feeType != null)
                {
                    int feePercent = overrideChecked
                        ? overridePercent ?? 0
                        : (within24Hours ? 100 : 0); // Use default policy if no override

                    cancellationFee = Math.Round(rate * feePercent / 100m, 2);

                    var existing = await _unitOfWork.Fee.GetAsync(f =>
                        f.ReservationId == res.ReservationId &&
                        f.FeeTypeId == feeType.Id &&
                        f.TriggerType == TriggerType.Triggered);

                    if (existing == null && cancellationFee > 0)
                    {
                        _unitOfWork.Fee.Add(new Fee
                        {
                            FeeTypeId = feeType.Id,
                            FeeTotal = cancellationFee,
                            ReservationId = res.ReservationId,
                            TriggerType = TriggerType.Triggered,
                            Notes = $"Cancellation Fee ({feePercent}%) - {reason}",
                            AppliedDate = DateTime.UtcNow
                        });
                    }
                }

                res.TotalDue = cancellationFee;
                res.AmountPaid = 0;
                res.Status = "Cancelled";

                if (res.LotId.HasValue)
                {
                    var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId.Value);
                    if (lot != null)
                    {
                        lot.IsAvailable = true;
                        _unitOfWork.Lot.Update(lot);
                    }
                }

                _unitOfWork.Reservation.Update(res);

                if (res.Guest != null)
                {
                    res.Guest.Balance = res.Guest.Reservations
                        .Where(r => r.Status != "Cancelled")
                        .Sum(r => Math.Max(0, r.TotalDue - r.AmountPaid));

                    _unitOfWork.Guest.Update(res.Guest);
                }

                await _unitOfWork.CommitAsync();
                TempData["Success"] = "Reservation cancelled successfully.";
                return RedirectToPage("./Index");
            }

            if (action == "save")
            {
                // === SAVE CHANGES ===
                var oldLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);
                var rate = (decimal)(res.Lot?.LotType?.Rate ?? 0);

                res.StartDate = ViewModel.Reservation.StartDate;
                res.EndDate = ViewModel.Reservation.EndDate;
                res.LotId = ViewModel.Reservation.LotId;
                res.LotTypeId = ViewModel.Reservation.LotTypeId;
                res.Duration = Math.Max(1, (res.EndDate - res.StartDate).Days);

                if (oldLot != null && oldLot.Id != res.LotId)
                {
                    oldLot.IsAvailable = true;
                    _unitOfWork.Lot.Update(oldLot);

                    var newLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId, includes: "LotType");
                    if (newLot != null)
                    {
                        newLot.IsAvailable = false;
                        res.Lot = newLot;
                        rate = (decimal)(newLot.LotType?.Rate ?? 0);
                        _unitOfWork.Lot.Update(newLot);
                    }
                }

                decimal newBaseTotal = (decimal)(res.Duration * rate);
                decimal manualFeeTotal = 0m;
                var role = User.FindFirstValue(ClaimTypes.Role);

                if (role == SD.AdminRole || role == SD.SuperAdminRole || role == SD.CampHostRole)
                {
                    var selectedFeeIds = Request.Form["SelectedManualFees"];

                    foreach (var feeIdStr in selectedFeeIds)
                    {
                        if (int.TryParse(feeIdStr, out int feeId))
                        {
                            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == feeId);
                            if (fee != null && fee.ReservationId == null)
                            {
                                fee.ReservationId = res.ReservationId;
                                fee.AppliedDate = DateTime.UtcNow;
                                fee.Notes = $"Manual Fee: {fee.FeeType?.FeeTypeName ?? "Unnamed"}";
                                manualFeeTotal += fee.FeeTotal;
                                _unitOfWork.Fee.Update(fee);
                            }
                        }
                    }
                }

                decimal balanceDifference = newBaseTotal - ViewModel.OriginalTotal;
                decimal taxableAmount = balanceDifference > 0 ? balanceDifference + manualFeeTotal : manualFeeTotal;
                decimal tax = taxableAmount * 0.0825m;

                res.TotalDue = ViewModel.OriginalTotal + balanceDifference + manualFeeTotal + tax;

                _unitOfWork.Reservation.Update(res);

                var guest = await _unitOfWork.Guest.GetAsync(
                    g => g.GuestId == res.GuestId,
                    includes: "Reservations");

                if (guest != null)
                {
                    guest.Balance = guest.Reservations
                        .Where(r => r.Status != "Cancelled")
                        .Sum(r => Math.Max(0, r.TotalDue - r.AmountPaid));

                    _unitOfWork.Guest.Update(guest);
                }

                await _unitOfWork.CommitAsync();
                TempData["Success"] = "Reservation and guest balance updated successfully.";
                return RedirectToPage("./Index");
            }

            return RedirectToPage("./Index");
        }



        public async Task<IActionResult> OnGetAvailableLotsAsync(int lotTypeId, int trailerLength, DateTime startDate, DateTime endDate)
        {
            var overlappingReservations = await _unitOfWork.Reservation.GetAllAsync(
                r => !(r.EndDate < startDate || r.StartDate > endDate));
            var reservedLotIds = overlappingReservations.Select(r => r.LotId).Distinct();

            var lots = await _unitOfWork.Lot.GetAllAsync(
                l => l.LotTypeId == lotTypeId &&
                     l.Length >= trailerLength &&
                     !reservedLotIds.Contains(l.Id) &&
                     l.IsAvailable && !l.IsArchived,
                includes: "LotType");

            return new JsonResult(lots.Select(l => new
            {
                id = l.Id,
                location = l.Location,
                lotTypeRate = l.LotType?.Rate ?? 0
            }));
        }
    }
}
