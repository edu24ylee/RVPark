using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using static ApplicationCore.Models.Reservation;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public ReservationController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? filter = null)
        {
            var reservations = await _unitOfWork.Reservation.GetAllAsync(
                includes: "Guest.User,Rv,Lot.LotType");

            if (filter == "active")
            {
                reservations = reservations
                    .Where(r => r.Status != "Completed" && r.Status != "Cancelled")
                    .ToList();
            }

            var result = reservations.Select(r => new
            {
                reservationId = r.ReservationId,
                guest = new
                {
                    user = new
                    {
                        firstName = r.Guest?.User?.FirstName ?? "",
                        lastName = r.Guest?.User?.LastName ?? ""
                    }
                },
                rv = new
                {
                    licensePlate = r.Rv?.LicensePlate ?? ""
                },
                lot = new
                {
                    location = r.Lot?.Location ?? ""
                },
                startDate = r.StartDate,
                endDate = r.EndDate,
                status = r.Status,
                totalDue = r.TotalDue,
                amountPaid = r.AmountPaid,
                remainingBalance = r.RemainingBalance
            }).ToList();

            return Json(new { data = result });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            if (reservation == null)
                return Json(new { success = false, message = "Invalid reservation data." });

            _unitOfWork.Reservation.Add(reservation);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, data = reservation });
        }

        [HttpPut("modify/{id}")]
        public async Task<IActionResult> ModifyReservation(int id, [FromBody] int newDuration)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(r => r.ReservationId == id);
            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found." });

            reservation.UpdateDuration(newDuration);
            _unitOfWork.Reservation.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, data = reservation });
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var cancelRequest = System.Text.Json.JsonSerializer.Deserialize<CancelRequestModel>(body);
            if (cancelRequest == null)
            {
                return Json(new { success = false, message = "Invalid cancellation data." });
            }

                  if (cancelRequest == null)
                return BadRequest("Cancellation request payload is null.");

            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == id, includes: "Lot.LotType,Guest.Reservations");

            if (reservation == null)
                return NotFound(new { success = false, message = "Reservation not found." });

            var feeType = await _unitOfWork.FeeType.GetAsync(f =>
                f.FeeTypeName == "Cancellation Fee" && f.TriggerType == TriggerType.Triggered);

            decimal rate = (decimal)(reservation.Lot?.LotType?.Rate ?? 0);
            reservation.Duration = 1;
            var hoursBeforeStart = (reservation.StartDate - DateTime.UtcNow).TotalHours;
            bool within24Hours = hoursBeforeStart <= 24;

            int feePercent = 0;
            if (cancelRequest.Override)
            {
                feePercent = cancelRequest.Percent ?? 0;
            }
            else if (within24Hours)
            {
                feePercent = 100;
            }

            decimal cancellationFee = Math.Round(rate * feePercent / 100m, 2);

            var existing = await _unitOfWork.Fee.GetAsync(f =>
                f.ReservationId == reservation.ReservationId &&
                f.FeeTypeId == feeType.Id &&
                f.TriggerType == TriggerType.Triggered);

            if (existing == null && cancellationFee > 0)
            {
                _unitOfWork.Fee.Add(new Fee
                {
                    FeeTypeId = feeType.Id,
                    FeeTotal = cancellationFee,
                    ReservationId = reservation.ReservationId,
                    TriggerType = TriggerType.Triggered,
                    Notes = $"Cancellation Fee ({feePercent}%) - {cancelRequest.Reason}",
                    AppliedDate = DateTime.UtcNow
                });
            }

            reservation.TotalDue = cancellationFee;
            reservation.AmountPaid = 0;
            reservation.Status = "Cancelled";

            if (reservation.LotId.HasValue)
            {
                var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == reservation.LotId.Value);
                if (lot != null)
                {
                    lot.IsAvailable = true;
                    _unitOfWork.Lot.Update(lot);
                }
            }

            _unitOfWork.Reservation.Update(reservation);

            if (reservation.Guest != null)
            {
                reservation.Guest.Balance = reservation.Guest.Reservations
                    .Where(r => r.Status != "Cancelled")
                    .Sum(r => Math.Max(0, r.TotalDue - r.AmountPaid));

                _unitOfWork.Guest.Update(reservation.Guest);
            }

            await _unitOfWork.CommitAsync();
            return Ok(new { success = true, message = "Reservation cancelled successfully." });
        }

        [HttpGet("guest/{guestId}")]
        public async Task<IActionResult> GetGuestReservations(int guestId)
        {
            var reservations = await _unitOfWork.Reservation.GetAllAsync(r => r.GuestId == guestId);
            return Json(new { success = true, data = reservations });
        }

        [HttpGet("availability/{lotId}")]
        [HttpGet("available-lots")]
        public async Task<IActionResult> GetAvailableLots(
            int lotTypeId,
            int trailerLength,
            DateTime startDate,
            DateTime endDate,
            int? lotId = null)
        {
            var overlappingReservations = await _unitOfWork.Reservation.GetAllAsync(
                r => !(r.EndDate < startDate || r.StartDate > endDate));

            var reservedLotIds = overlappingReservations
                .Where(r => r.LotId.HasValue)
                .Select(r => r.LotId.Value)
                .ToHashSet();

            var allLots = await _unitOfWork.Lot.GetAllAsync(
                l => l.LotTypeId == lotTypeId &&
                     l.Length >= trailerLength &&
                     l.IsAvailable && !l.IsArchived,
                includes: "LotType");

            if (lotId.HasValue)
            {
                bool available = allLots.Any(l => l.Id == lotId.Value && !reservedLotIds.Contains(l.Id));
                return Json(new { success = true, available });
            }

            var availableLots = allLots
                .Where(l => !reservedLotIds.Contains(l.Id))
                .Select(l => new
                {
                    id = l.Id,
                    location = l.Location,
                    lotTypeRate = l.LotType?.Rate ?? 0
                });

            return Json(new { success = true, data = availableLots });
        }

        [HttpPost("status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(r => r.ReservationId == id);
            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found." });

            if (request.Status == "Cancelled")
                return Json(new { success = false, message = "Cannot change status to 'Cancelled' here." });

            reservation.Status = request.Status;
            _unitOfWork.Reservation.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true });
        }

        [HttpGet("guest/{guestId}/rv")]
        public async Task<IActionResult> GetGuestRv(int guestId)
        {
            var guest = await _unitOfWork.Guest.GetAsync(g => g.GuestId == guestId, includes: "Rvs");

            var rv = guest?.Rvs?.FirstOrDefault();
            if (rv == null)
                return NotFound(new { message = "RV not found for guest." });

            return Ok(new
            {
                rvId = rv.RvId,
                length = rv.Length
            });
        }
    }


    public class CancelRequestModel
    {
        public bool Override { get; set; }
        public int? Percent { get; set; }
        public string? Reason { get; set; }
    }



    public class StatusUpdateRequest
    {
        public string Status { get; set; } = null!;
    }
}