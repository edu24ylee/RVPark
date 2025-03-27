using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ApplicationCore.Models;
using Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

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
            "Active",
            "Cancelled",
            "Confirmed",
            "Completed",
            "Pending"
        };

        [BindProperty, Required(ErrorMessage = "First name is required.")]
        public string GuestFirstName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Last name is required.")]
        public string GuestLastName { get; set; } = string.Empty;

        [BindProperty, Required(ErrorMessage = "Trailer length is required.")]
        public decimal Length { get; set; }

        [BindProperty]
        public Reservation Reservation { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id != null)
            {
                Reservation = await _unitOfWork.Reservation.GetAsync(
                    r => r.ReservationId == id,
                    includes: "Guest.User,Rv,Lot"
                ) ?? new Reservation();
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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

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

            var rv = new RV
            {
                Guest = guest,
                Length = (int)Length,
                Make = "Unknown",
                Model = "Unknown",
                LicensePlate = "TEMP",
                Description = "User Input"
            };
            _unitOfWork.RV.Add(rv);
            await _unitOfWork.CommitAsync();

            Reservation.GuestId = guest.GuestID;
            Reservation.RvId = rv.RvID;
            Reservation.Duration = (Reservation.EndDate - Reservation.StartDate).Days;

            if (Reservation.Duration <= 0)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                return Page();
            }

            var lots = await _unitOfWork.Lot.GetAllAsync();
            var selectedLot = lots
                .Where(l => l.IsAvailable && l.Length >= rv.Length)
                .OrderBy(l => l.Length)
                .FirstOrDefault();

            if (selectedLot == null)
            {
                ModelState.AddModelError("", "No available lot found for the trailer size.");
                return Page();
            }

            Reservation.LotId = selectedLot.Id;

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
