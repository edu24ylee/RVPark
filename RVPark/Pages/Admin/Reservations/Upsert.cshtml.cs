using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ApplicationCore.Models;
using Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.Reservations
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<string> StatusOptions { get; } = new()
        {
            "Active", "Cancelled", "Confirmed", "Completed", "Pending"
        };

        [BindProperty, Required(ErrorMessage = "First name is required.")]
        public string GuestFirstName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Last name is required.")]
        public string GuestLastName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Trailer length is required.")]
        public decimal Length { get; set; }

        [BindProperty]
        public Reservation Reservation { get; set; } = new();

        public IEnumerable<SelectListItem> AvailableLots { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id != null)
            {
                Reservation = await _unitOfWork.Reservation.GetAsync(
                    r => r.ReservationId == id,
                    includes: "Guest.User,Rv,Lot"
                ) ?? new Reservation();

                GuestFirstName = Reservation.Guest?.User?.FirstName ?? "";
                GuestLastName = Reservation.Guest?.User?.LastName ?? "";
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
                    Status = "Active"
                };
            }

            AvailableLots = _unitOfWork.Lot.GetAll(l => l.IsAvailable)
                .Select(l => new SelectListItem
                {
                    Text = $"Lot #{l.Id} - {l.Description}",
                    Value = l.Id.ToString()
                });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Reservation.Duration = (Reservation.EndDate - Reservation.StartDate).Days;

            if (Reservation.Duration <= 0)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                return Page();
            }

            if (Reservation.ReservationId == 0)
            {
                var user = new User
                {
                    FirstName = GuestFirstName,
                    LastName = GuestLastName,
                    Email = "placeholder@email.com",
                    Phone = "000-000-0000",
                    IsActive = true
                };

                var guest = new Guest { User = user, DodId = 0 };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                var rv = new Rv
                {
                    Guest = guest,
                    Length = (int)Length,
                    Make = "Unknown",
                    Model = "Unknown",
                    LicensePlate = "TEMP",
                    Description = "User Input"
                };
                _unitOfWork.Rv.Add(rv);
                await _unitOfWork.CommitAsync();

                Reservation.GuestId = guest.GuestId;
                Reservation.RvId = rv.RvId;
            }
            else
            {
                var existingReservation = await _unitOfWork.Reservation.GetAsync(
                    r => r.ReservationId == Reservation.ReservationId,
                    includes: "Guest.User,Rv"
                );

                if (existingReservation == null)
                {
                    ModelState.AddModelError("", "Reservation not found.");
                    return Page();
                }

                Reservation.GuestId = existingReservation.GuestId;
                Reservation.RvId = existingReservation.RvId;

                existingReservation.Guest.User.FirstName = GuestFirstName;
                existingReservation.Guest.User.LastName = GuestLastName;
                existingReservation.Rv.Length = (int)Length;

                _unitOfWork.User.Update(existingReservation.Guest.User);
                _unitOfWork.Rv.Update(existingReservation.Rv);
                await _unitOfWork.CommitAsync();
            }

            var lots = await _unitOfWork.Lot.GetAllAsync();
            var reservations = await _unitOfWork.Reservation.GetAllAsync();

            var availableLots = lots
                .Where(l => l.IsAvailable && (decimal)l.Length >= Length)
                .Where(lot =>
                    !reservations.Any(r =>
                        r.LotId == lot.Id &&
                        r.ReservationId != Reservation.ReservationId &&
                        (
                            (Reservation.StartDate >= r.StartDate && Reservation.StartDate < r.EndDate) ||
                            (Reservation.EndDate > r.StartDate && Reservation.EndDate <= r.EndDate) ||
                            (Reservation.StartDate <= r.StartDate && Reservation.EndDate >= r.EndDate)
                        )
                    )
                )
                .OrderBy(l => l.Length)
                .ToList();

            var selectedLot = availableLots.FirstOrDefault();

            if (selectedLot == null)
            {
                ModelState.AddModelError("", "No available lot found for the trailer size and date range.");
                return Page();
            }

            Reservation.LotId = selectedLot.Id;

            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (Reservation.ReservationId == 0)
                    _unitOfWork.Reservation.Add(Reservation);
                else
                    _unitOfWork.Reservation.Update(Reservation);

                await _unitOfWork.CommitAsync();
                TempData["Success"] = "Reservation saved successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving reservation: {ex.Message}");
                return Page();
            }
        }
    }
}
