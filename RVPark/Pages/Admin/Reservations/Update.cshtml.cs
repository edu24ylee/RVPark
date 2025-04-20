using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static ApplicationCore.Models.Reservation;

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
        [BindProperty(SupportsGet = true)] public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Page("/Admin/Reservations/Update", new { id });

            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == id,
                includes: "Guest.User,Rv,Lot.LotType");

            if (reservation == null)
                return NotFound();

            var lotTypes = await _unitOfWork.LotType.GetAllAsync();
            var availableLots = await _unitOfWork.Lot.GetAllAsync(
                l => l.IsAvailable || l.Id == reservation.LotId,
                includes: "LotType");


            var manualFeeTypes = await _unitOfWork.FeeType.GetAllAsync(
                f => f.TriggerType == TriggerType.Manual && !f.IsArchived);

            var manualFeeOptions = manualFeeTypes.Select(ft => new ManualFeeOptionViewModel
            {
                Id = ft.Id,
                FeeTypeName = ft.FeeTypeName,
                FeeTotal = 0m 
            }).ToList();

            var existingFees = await _unitOfWork.Fee.GetAllAsync(
                f => f.ReservationId == reservation.ReservationId && !f.IsArchived,
                includes: "FeeType");
            reservation.Duration = (reservation.EndDate - reservation.StartDate).Days;

            ViewModel = new ReservationUpdateModel
            {
                Reservation = reservation,
                GuestName = $"{reservation.Guest.User.FirstName} {reservation.Guest.User.LastName}",
                Rv = reservation.Rv,
                LotTypes = lotTypes.ToList(),
                AvailableLots = availableLots.ToList(),
                ManualFeeOptions = manualFeeOptions,
                ExistingFees = existingFees.ToList(),
                OriginalTotal = reservation.CalculateTotal((decimal)(reservation.Lot?.LotType?.Rate ?? 0))
            };

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var action = Request.Form["action"];

            var res = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == ViewModel.Reservation.ReservationId,
                includes: "Lot.LotType,Guest.Reservations");

            if (res == null) return NotFound();

            if (action == "confirmCancel")
            {
                var reason = Request.Form["cancelOverrideReason"];
                var overrideChecked = Request.Form["cancelOverride"] == "on";
                var overrideStr = Request.Form["overridePercent"];
                int? overridePercent = int.TryParse(overrideStr, out var parsedPercent) ? parsedPercent : null;

                var feeTypes = await _unitOfWork.FeeType.GetAllAsync();
                var feeType = feeTypes.FirstOrDefault(f =>
                    f.FeeTypeName == "Cancellation Fee" && f.TriggerType == TriggerType.Triggered);

                decimal rate = res.Lot?.LotType?.Rate ?? 0;
                decimal cancellationFee = 0m;

                var hoursBeforeStart = (res.StartDate - DateTime.UtcNow).TotalHours;
                bool within24Hours = hoursBeforeStart <= 24;

                int feePercent = 0;

                if (within24Hours)
                {
                    feePercent = overrideChecked && overridePercent.HasValue ? overridePercent.Value : 100;
                    cancellationFee = Math.Round(rate * (feePercent / 100m), 2);

                    if (feeType != null && cancellationFee > 0)
                    {
                        var existing = await _unitOfWork.Fee.GetAsync(f =>
                            f.ReservationId == res.ReservationId &&
                            f.FeeTypeId == feeType.Id &&
                            f.TriggerType == TriggerType.Triggered);

                        if (existing == null)
                        {
                            _unitOfWork.Fee.Add(new Fee
                            {
                                FeeTypeId = feeType.Id,
                                FeeTotal = cancellationFee,
                                ReservationId = res.ReservationId,
                                TriggerType = TriggerType.Triggered,
                                AppliedDate = DateTime.UtcNow,
                                Notes = $"Cancellation Fee ({feePercent}%) - {reason}"
                            });
                        }
                    }
                }

                res.BaseTotal = 0;
                res.ManualFeeTotal = 0;
                res.TaxTotal = 0;
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
                var oldLot = await _unitOfWork.Lot.GetAsync(l => l.Id == res.LotId);
                var rate = res.Lot?.LotType?.Rate ?? 0;

                res.StartDate = ViewModel.Reservation.StartDate;
                res.EndDate = ViewModel.Reservation.EndDate;
                res.LotId = ViewModel.Reservation.LotId;
                res.LotTypeId = ViewModel.Reservation.LotTypeId;
                res.NumberOfAdults = ViewModel.Reservation.NumberOfAdults;
                res.NumberOfPets = ViewModel.Reservation.NumberOfPets;
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
                        rate = newLot.LotType?.Rate ?? 0;
                        _unitOfWork.Lot.Update(newLot);
                    }
                }

                decimal newBaseTotal = (decimal)res.Duration * rate;
                decimal manualFeeTotal = 0m;

                var role = User.FindFirstValue(ClaimTypes.Role);
                if (role == SD.AdminRole || role == SD.SuperAdminRole || role == SD.CampHostRole)
                {
                    var selectedFeeIds = Request.Form["SelectedManualFees"];

                    foreach (var feeIdStr in selectedFeeIds)
                    {
                        if (int.TryParse(feeIdStr, out int feeId))
                        {
                            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == feeId && f.TriggerType == TriggerType.Manual);
                            if (fee != null)
                            {
                                manualFeeTotal += fee.FeeTotal ?? 0m;

                                fee.ReservationId = res.ReservationId;
                                fee.AppliedDate = DateTime.UtcNow;

                                _unitOfWork.Fee.Update(fee);
                            }
                        }
                    }
                }

           
            decimal balanceDifference = newBaseTotal - ViewModel.OriginalTotal;
                decimal taxableAmount = balanceDifference > 0 ? balanceDifference + manualFeeTotal : manualFeeTotal;
                decimal tax = taxableAmount * 0.0825m;

                res.BaseTotal = newBaseTotal;
                res.ManualFeeTotal = manualFeeTotal;
                res.TaxTotal = tax;

                decimal grandTotal = ViewModel.OriginalTotal + balanceDifference + manualFeeTotal + tax;
                res.TotalDue = Math.Max(0, grandTotal - res.AmountPaid);

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
