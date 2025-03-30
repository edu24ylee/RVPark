using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

        [BindProperty(SupportsGet = true)]
        public int? SelectedLotTypeId { get; set; }

        public SelectList LotTypeOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList LotOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public decimal UpdatedTotal { get; set; }
        public decimal BalanceDifference { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Console.WriteLine($"[DEBUG] OnGetAsync called with ReservationId: {id}");

            Reservation = await _context.Reservation
                .Include(r => r.Guest).ThenInclude(g => g.User)
                .Include(r => r.Lot).ThenInclude(l => l.LotType)
                .Include(r => r.Rv)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (Reservation == null)
            {
                Console.WriteLine("[DEBUG] Reservation not found.");
                return NotFound();
            }

            SelectedLotTypeId = Reservation.Lot?.LotTypeId;

            await LoadDropdownsAsync(Reservation.Rv.Length, Reservation.StartDate, Reservation.EndDate);

            UpdatedTotal = Reservation.CalculateTotal((decimal)Reservation.Lot.LotType.Rate);
            BalanceDifference = 0;

            Console.WriteLine($"[DEBUG] Initial total: {UpdatedTotal:C}");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Reload reservation with associations
            var reservationInDb = await _context.Reservation
                .Include(r => r.Guest).ThenInclude(g => g.User)
                .Include(r => r.Lot).ThenInclude(l => l.LotType)
                .Include(r => r.Rv)
                .FirstOrDefaultAsync(r => r.ReservationId == Reservation.ReservationId);

            if (reservationInDb == null)
                return NotFound();

            // Required fields debug check
            if (Reservation.GuestId == 0 || Reservation.RvId == 0 || Reservation.LotId == 0 || string.IsNullOrEmpty(Reservation.Status))
            {
                ModelState.AddModelError(string.Empty, "Required fields missing.");
                await LoadDropdownsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);
                return Page();
            }

            // Update fields
            reservationInDb.StartDate = Reservation.StartDate;
            reservationInDb.EndDate = Reservation.EndDate;
            reservationInDb.Duration = (Reservation.EndDate - Reservation.StartDate).Days;
            reservationInDb.Status = Reservation.Status;
            reservationInDb.LotId = Reservation.LotId;
            reservationInDb.GuestId = Reservation.GuestId;
            reservationInDb.RvId = Reservation.RvId;

            if (reservationInDb.Duration <= 0)
            {
                ModelState.AddModelError(string.Empty, "End date must be after start date.");
                await LoadDropdownsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);
                return Page();
            }

            // Check for Lot Availability
            var isAvailable = await _context.Reservation
                .Where(r => r.LotId == Reservation.LotId && r.ReservationId != Reservation.ReservationId)
                .AllAsync(r => r.EndDate <= reservationInDb.StartDate || r.StartDate >= reservationInDb.EndDate);

            if (!isAvailable)
            {
                ModelState.AddModelError(string.Empty, "Lot is not available for these dates.");
                await LoadDropdownsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);
                return Page();
            }

            // Recalculate Cost
            var oldRate = (decimal)reservationInDb.Lot.LotType.Rate;
            var newLot = await _context.Lot.Include(l => l.LotType).FirstAsync(l => l.Id == Reservation.LotId);
            var newRate = newLot.LotType.Rate;
            var newTotal = reservationInDb.Duration * newRate;

            UpdatedTotal = (decimal)newTotal;
            BalanceDifference = (decimal)newTotal - reservationInDb.CalculateTotal(oldRate);


            await _context.SaveChangesAsync();
            await LoadDropdownsAsync(reservationInDb.Rv.Length, Reservation.StartDate, Reservation.EndDate);

            return Page();
        }


        public async Task<IActionResult> OnPostCancelAsync()
        {
            Console.WriteLine("[DEBUG] Cancel handler triggered.");

            var reservation = await _context.Reservation
                .Include(r => r.Lot).ThenInclude(l => l.LotType)
                .FirstOrDefaultAsync(r => r.ReservationId == Reservation.ReservationId);

            if (reservation == null)
            {
                Console.WriteLine("[DEBUG] Reservation not found for cancellation.");
                return NotFound();
            }

            reservation.CancelReservation();

            var now = DateTime.UtcNow;
            var hoursUntilStart = (reservation.StartDate - now).TotalHours;

            if (hoursUntilStart <= 24)
            {
                Console.WriteLine("[DEBUG] Within 24 hours of reservation. Charging cancellation fee.");

                var feeType = await _context.FeeType.FirstOrDefaultAsync(f => f.FeeTypeName == "Cancellation Fee");

                if (feeType != null)
                {
                    decimal ratePerDay = (decimal)reservation.Lot.LotType.Rate;
                    decimal total = reservation.Duration * ratePerDay;
                    decimal cancellationFeeAmount = total * 0.20m;

                    Console.WriteLine($"[DEBUG] Cancellation fee: {cancellationFeeAmount:C}");

                    var fee = new Fee
                    {
                        FeeTypeId = feeType.Id,
                        FeeTotal = cancellationFeeAmount,
                        TriggeringPolicyId = null,
                        TransactionId = null
                    };

                    _context.Fee.Add(fee);
                }
                else
                {
                    Console.WriteLine("[DEBUG] FeeType 'Cancellation Fee' not found.");
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Reservations/Index");
        }

        private async Task LoadDropdownsAsync(int trailerLength, DateTime start, DateTime end)
        {
            Console.WriteLine("[DEBUG] Loading dropdowns...");

            var lotTypes = await _context.LotType.OrderBy(lt => lt.Name).ToListAsync();
            LotTypeOptions = new SelectList(lotTypes, "Id", "Name", SelectedLotTypeId);

            if (SelectedLotTypeId.HasValue)
            {
                var allLots = await _context.Lot.Include(l => l.LotType)
                    .Where(l => l.Length >= trailerLength && l.LotTypeId == SelectedLotTypeId.Value)
                    .ToListAsync();

                var reservedLotIds = await _context.Reservation
                    .Where(r =>
                        r.Status != "Cancelled" &&
                        r.StartDate < end &&
                        r.EndDate > start &&
                        r.ReservationId != Reservation.ReservationId)
                    .Select(r => r.LotId.Value)
                    .ToListAsync();

                var availableLots = allLots.Where(l => !reservedLotIds.Contains(l.Id)).ToList();

                if (Reservation.LotId.HasValue && !availableLots.Any(l => l.Id == Reservation.LotId))
                {
                    var currentLot = await _context.Lot.Include(l => l.LotType)
                                                       .FirstOrDefaultAsync(l => l.Id == Reservation.LotId.Value);
                    if (currentLot != null)
                        availableLots.Add(currentLot);
                }

                LotOptions = new SelectList(
                    availableLots
                        .OrderBy(l => l.Location)
                        .Select(l => new
                        {
                            l.Id,
                            DisplayText = $"{l.Location} ({l.LotType.Name})"
                        }),
                    "Id",
                    "DisplayText",
                    Reservation.LotId
                );
            }
        }
    }
}
