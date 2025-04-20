using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RVPark.Pages.Admin.Reservations
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            GuestOptions = new List<SelectListItem>();
            LotTypeOptions = new List<LotType>();
        }

        [BindProperty] public Reservation Reservation { get; set; } = new();
        [BindProperty] public int? SelectedGuestId { get; set; }
        [BindProperty, Required] public string GuestFirstName { get; set; } = string.Empty;
        [BindProperty, Required] public string GuestLastName { get; set; } = string.Empty;
        [BindProperty, Required] public decimal Length { get; set; }
        [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

        public List<SelectListItem> GuestOptions { get; set; }
        public List<LotType> LotTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string? returnUrl = null, int? guestId = null, int? rvId = null)
        {
            ReturnUrl = returnUrl;

            var guests = await _unitOfWork.Guest.GetAllAsync(includes: "User");
            GuestOptions = guests.Select(g => new SelectListItem
            {
                Value = g.GuestId.ToString(),
                Text = $"{g.User.FirstName} {g.User.LastName}"
            }).ToList();

            LotTypeOptions = (await _unitOfWork.LotType.GetAllAsync()).ToList();

            if (id.HasValue)
            {
                Reservation = await _unitOfWork.Reservation.GetAsync(
                    r => r.ReservationId == id.Value,
                    includes: "Guest.User,Rv,Lot.LotType");

                if (Reservation == null)
                    return NotFound();

                SelectedGuestId = Reservation.GuestId;
                GuestFirstName = Reservation.Guest?.User.FirstName ?? string.Empty;
                GuestLastName = Reservation.Guest?.User.LastName ?? string.Empty;
                Length = Reservation.Rv?.Length ?? 0;
            }
            else if (guestId.HasValue)
            {
                var guest = await _unitOfWork.Guest.GetAsync(g => g.GuestId == guestId.Value, includes: "User,Rvs");
                var rv = rvId.HasValue
                    ? await _unitOfWork.Rv.GetAsync(r => r.RvId == rvId.Value)
                    : guest?.Rvs?.FirstOrDefault();

                if (guest != null && guest.User != null)
                {
                    Reservation.GuestId = guest.GuestId;
                    Reservation.RvId = rv?.RvId ?? 0;
                    SelectedGuestId = guest.GuestId;
                    GuestFirstName = guest.User.FirstName;
                    GuestLastName = guest.User.LastName;
                    Length = rv?.Length ?? 0;
                }

                Reservation.StartDate = DateTime.UtcNow.Date;
                Reservation.EndDate = Reservation.StartDate.AddDays(1);
                Reservation.Duration = 1;
                Reservation.Status = "Pending";
                Reservation.NumberOfAdults = 1;
                Reservation.NumberOfPets = 0;
            }
            else
            {
                Reservation = new Reservation
                {
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(1),
                    Duration = 1,
                    Status = "Pending",
                    NumberOfAdults = 1,
                    NumberOfPets = 0
                };

                GuestFirstName = string.Empty;
                GuestLastName = string.Empty;
                Length = 0;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Reservation.Duration = (Reservation.EndDate - Reservation.StartDate).Days;

            if (Reservation.Duration <= 0)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                return await OnGetAsync(Reservation.ReservationId == 0 ? null : Reservation.ReservationId);
            }

            if (SelectedGuestId.HasValue)
            {
                var guest = await _unitOfWork.Guest.GetAsync(g => g.GuestId == SelectedGuestId.Value, includes: "User");

                if (guest == null)
                {
                    ModelState.AddModelError("", "Selected guest not found.");
                    return await OnGetAsync(null);
                }

                Reservation.GuestId = guest.GuestId;
                Reservation.RvId = 0; // Don't rely on RV existence
            }
            else
            {
                var user = new User
                {
                    FirstName = GuestFirstName,
                    LastName = GuestLastName,
                    Email = "placeholder@email.com",
                    Phone = "000-000-0000",
                    IsActive = true
                };

                var guest = new Guest { User = user };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                Reservation.GuestId = guest.GuestId;
                Reservation.RvId = 0;
            }

            var allLots = await _unitOfWork.Lot.GetAllAsync(l => l.IsAvailable && !l.IsArchived);
            var reservations = await _unitOfWork.Reservation.GetAllAsync();

            var availableLots = allLots
                .Where(l =>
                    (decimal)l.Length >= Length &&
                    !reservations.Any(r =>
                        r.LotId == l.Id &&
                        r.ReservationId != Reservation.ReservationId &&
                        (
                            (Reservation.StartDate >= r.StartDate && Reservation.StartDate < r.EndDate) ||
                            (Reservation.EndDate > r.StartDate && Reservation.EndDate <= r.EndDate) ||
                            (Reservation.StartDate <= r.StartDate && Reservation.EndDate >= r.EndDate)
                        )
                    )
                ).OrderBy(l => l.Length).ToList();

            if (availableLots.All(l => l.Id != Reservation.LotId))
            {
                var fallback = availableLots.FirstOrDefault();
                if (fallback != null)
                    Reservation.LotId = fallback.Id;
                else
                {
                    ModelState.AddModelError("", "No lot available for the selected range and trailer size.");
                    return await OnGetAsync(null);
                }
            }

            var selectedLot = await _unitOfWork.Lot.GetAsync(l => l.Id == Reservation.LotId, includes: "LotType");
            var rate = selectedLot?.LotType?.Rate ?? 0;
            Reservation.BaseTotal = (decimal)(rate * Reservation.Duration);
            Reservation.TaxTotal = Reservation.BaseTotal * 0.0825m;
            Reservation.TotalDue = Reservation.BaseTotal + Reservation.TaxTotal;
            Reservation.AmountPaid = 0;

            if (!ModelState.IsValid)
                return await OnGetAsync(null);

            if (Reservation.ReservationId == 0)
                _unitOfWork.Reservation.Add(Reservation);
            else
                _unitOfWork.Reservation.Update(Reservation);

            await _unitOfWork.CommitAsync();

            TempData["Success"] = "Reservation saved successfully.";
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
