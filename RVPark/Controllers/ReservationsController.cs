using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

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
                includes: "Guest.User,Rv,Lot"
            );

            if (filter == "active")
            {
                reservations = reservations
                    .Where(r => r.Status != "Cancelled" && r.Status != "Completed")
                    .ToList();
            }

            var result = reservations.Select(r => new
            {
                reservationId = r.ReservationId,
                guest = new
                {
                    user = new
                    {
                        firstName = r.Guest?.User?.FirstName ?? "N/A",
                        lastName = r.Guest?.User?.LastName ?? "N/A"
                    }
                },
                rv = new
                {
                    licensePlate = r.Rv?.LicensePlate ?? "N/A"
                },
                lot = new
                {
                    location = r.Lot?.Location ?? "N/A"
                },
                startDate = r.StartDate,
                endDate = r.EndDate,
                status = r.Status,
                totalDue = r.TotalDue,
                amountPaid = r.AmountPaid,
                remainingBalance = r.OutstandingBalance
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(r => r.ReservationId == id);
            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found." });

            _unitOfWork.Reservation.Delete(reservation);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Reservation deleted successfully." });
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id, [FromBody] CancelReservationRequest request)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(
                r => r.ReservationId == id,
                includes: "Lot.LotType"
            );

            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found." });

            var feeType = await _unitOfWork.FeeType.GetAsync(f =>
                f.FeeTypeName == "Cancellation Fee" &&
                f.TriggerType == TriggerType.Triggered);

            decimal rate = (decimal)(reservation.Lot?.LotType?.Rate ?? 0);
            decimal cancellationFee = 0m;

            var hoursBeforeStart = (reservation.StartDate - DateTime.UtcNow).TotalHours;
            bool within24Hours = hoursBeforeStart <= 24;

            int feePercent = request.Override
                ? request.Percent ?? 0
                : (within24Hours ? 100 : 0);

            cancellationFee = Math.Round(rate * feePercent / 100m, 2);

            if (feeType != null && cancellationFee > 0)
            {
                _unitOfWork.Fee.Add(new Fee
                {
                    FeeTypeId = feeType.Id,
                    FeeTotal = cancellationFee,
                    ReservationId = reservation.ReservationId,
                    TriggerType = TriggerType.Triggered,
                    AppliedDate = DateTime.UtcNow,
                    Notes = $"Cancellation Fee ({feePercent}%) - {request.Reason}"
                });
            }

            reservation.Status = "Cancelled";
            reservation.AmountPaid = 0;
            reservation.TotalDue = cancellationFee;

            _unitOfWork.Reservation.Update(reservation);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Reservation cancelled successfully." });
        }

        [HttpGet("guest/{guestId}")]
        public async Task<IActionResult> GetGuestReservations(int guestId)
        {
            var reservations = await _unitOfWork.Reservation.GetAllAsync(r => r.GuestId == guestId);
            return Json(new { success = true, data = reservations });
        }

        [HttpGet("availability/{lotId}")]
        public async Task<IActionResult> CheckAvailability(int lotId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var reservations = await _unitOfWork.Reservation.GetAllAsync(r =>
                r.LotId == lotId && r.StartDate < endDate && r.EndDate > startDate);

            return Json(new { success = true, available = !reservations.Any() });
        }
        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetStatus(int id)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(r => r.ReservationId == id);
            if (reservation == null)
                return NotFound();

            return Json(new { status = reservation.Status });
        }

    }

    public class CancelReservationRequest
    {
        public bool Override { get; set; }
        public int? Percent { get; set; }
        public string? Reason { get; set; }
    }
}
