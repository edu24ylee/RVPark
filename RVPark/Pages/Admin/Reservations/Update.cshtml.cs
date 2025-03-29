using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace RVPark.Pages.Admin.Reservations
{
    public class UpdateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UpdateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reservation Reservation { get; set; } = new Reservation();

        public SelectList AvailableLots { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public decimal UpdatedTotal { get; set; }
        public decimal BalanceDifference { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reservation = await _context.Reservation
                .Include(r => r.Guest).ThenInclude(g => g.User)
                .Include(r => r.Lot).ThenInclude(l => l.LotType)
                .Include(r => r.Rv)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (Reservation == null)
                return NotFound();

            await PopulateAvailableLotsAsync(Reservation.Rv.Length, Reservation.StartDate, Reservation.EndDate);
            UpdatedTotal = Reservation.CalculateTotal((decimal)Reservation.Lot.LotType.Rate);
            BalanceDifference = 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var reservationInDb = await _context.Reservation
                .Include(r => r.Lot).ThenInclude(l => l.LotType)
                .Include(r => r.Rv)
                .FirstOrDefaultAsync(r => r.ReservationId == Reservation.ReservationId);

            if (reservationInDb == null)
                return NotFound();

            var originalDuration = reservationInDb.Duration;
            var originalRate = reservationInDb.Lot.LotType.Rate;
            var originalTotal = originalDuration * originalRate;

            reservationInDb.StartDate = Reservation.StartDate;
            reservationInDb.EndDate = Reservation.EndDate;
            reservationInDb.Duration = (Reservation.EndDate - Reservation.StartDate).Days;

            if (reservationInDb.Duration <= 0)
            {
                ModelState.AddModelError(string.Empty, "End date must be after start date.");
                await PopulateAvailableLotsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);
                return Page();
            }

            var isAvailable = await _context.Reservation
                .Where(r => r.LotId == Reservation.LotId && r.ReservationId != Reservation.ReservationId)
                .AllAsync(r =>
                    r.EndDate <= reservationInDb.StartDate || r.StartDate >= reservationInDb.EndDate);

            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Selected lot is not available for the new dates.");
                await PopulateAvailableLotsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);
                return Page();
            }

            reservationInDb.LotId = Reservation.LotId;

            var newLot = await _context.Lot.Include(l => l.LotType).FirstAsync(l => l.Id == Reservation.LotId);
            var newRate = newLot.LotType.Rate;
            var newDuration = reservationInDb.Duration;
            var newTotal = newDuration * newRate;

            UpdatedTotal = (decimal)newTotal;
            BalanceDifference = (decimal)newTotal - (decimal)originalTotal;

            await _context.SaveChangesAsync();

            await PopulateAvailableLotsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);

            return Page();

        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var reservation = await _context.Reservation.FindAsync(Reservation.ReservationId);

            if (reservation == null)
                return NotFound();

            reservation.CancelReservation();
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Reservations/Index");
        }

        private async Task PopulateAvailableLotsAsync(int trailerLength, DateTime start, DateTime end)
        {
            var allLots = await _context.Lot
                .Include(l => l.LotType)
                .Where(l => l.Length >= trailerLength)
                .ToListAsync();

            var reservedLotIds = await _context.Reservation
                .Where(r =>
                    r.Status != "Cancelled" &&
                    r.StartDate < end &&
                    r.EndDate > start &&
                    r.ReservationId != Reservation.ReservationId)
                .Select(r => r.LotId.Value)
                .ToListAsync();

            var available = allLots.Where(l => !reservedLotIds.Contains(l.Id)).ToList();


            if (Reservation.LotId.HasValue && !available.Any(l => l.Id == Reservation.LotId))
            {
                var currentLot = await _context.Lot.Include(l => l.LotType)
                                                   .FirstOrDefaultAsync(l => l.Id == Reservation.LotId.Value);
                if (currentLot != null)
                    available.Add(currentLot);
            }

            AvailableLots = new SelectList(available.OrderBy(l => l.Location), "Id", "Location", Reservation.LotId);
        }
    }
}
