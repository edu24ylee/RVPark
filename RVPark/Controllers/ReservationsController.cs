using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;

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
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _unitOfWork.Reservation.GetAllAsync(
                includes: "Guest.User,Rv,Lot"
            );

            return Json(new { data = reservations });
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

        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _unitOfWork.Reservation.GetAsync(r => r.ReservationId == id);
            if (reservation == null)
                return Json(new { success = false, message = "Reservation not found." });

            reservation.CancelReservation();
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
            var reservations = await _unitOfWork.Reservation.GetAllAsync(r => r.LotId == lotId &&
                r.StartDate < endDate && r.EndDate > startDate);
            return Json(new { success = true, available = !reservations.Any() });
        }
    }
}
