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

        [BindProperty, Required(ErrorMessage = "First name is required.")] public string GuestFirstName { get; set; } = string.Empty;
        [BindProperty, Required(ErrorMessage = "Last name is required.")] public string GuestLastName { get; set; } = string.Empty;
        [BindProperty, Required(ErrorMessage = "Trailer length is required.")] public decimal Length { get; set; }

        public List<SelectListItem> GuestOptions { get; set; }
        public List<LotType> LotTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
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
                    includes: "Guest.User,Rv,Lot") ?? new Reservation();

                SelectedGuestId = Reservation.GuestId;
                GuestFirstName = Reservation.Guest?.User.FirstName ?? "";
                GuestLastName = Reservation.Guest?.User.LastName ?? "";
                Length = Reservation.Rv?.Length ?? 0;
            }
            else
            {
                var today = DateTime.UtcNow.Date;
                Reservation = new Reservation
                {
                    StartDate = today,
                    EndDate = today.AddDays(1),
                    Duration = 1,
                    Status = "Active",
                    NumberOfAdults = 1,
                    NumberOfPets = 0
                };
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

            // === Guest/RV handling ===
            Guest? guest;
            Rv? rv;

            if (SelectedGuestId.HasValue)
            {
                guest = await _unitOfWork.Guest.GetAsync(
                    g => g.GuestId == SelectedGuestId.Value,
                    includes: "User,RVs");

                if (guest == null)
                {
                    ModelState.AddModelError("", "Selected guest not found.");
                    return await OnGetAsync(null);
                }

                rv = guest.Rvs?.FirstOrDefault();
                if (rv == null)
                {
                    rv = new Rv
                    {
                        GuestId = guest.GuestId,
                        Length = (int)Length,
                        Make = "Unknown",
                        Model = "Unknown",
                        LicensePlate = "TEMP",
                        Description = "Auto-created during reservation"
                    };
                    _unitOfWork.Rv.Add(rv);
                    await _unitOfWork.CommitAsync();
                }

                Reservation.GuestId = guest.GuestId;
                Reservation.RvId = rv.RvId;
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

                guest = new Guest { User = user };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                rv = new Rv
                {
                    GuestId = guest.GuestId,
                    Length = (int)Length,
                    Make = "Unknown",
                    Model = "Unknown",
                    LicensePlate = "TEMP",
                    Description = "User Provided"
                };
                _unitOfWork.Rv.Add(rv);
                await _unitOfWork.CommitAsync();

                Reservation.GuestId = guest.GuestId;
                Reservation.RvId = rv.RvId;
            }

            // === Assign Lot ===
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

            // === Save ===
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
    }
}
