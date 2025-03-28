using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationReportController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public ReservationReportController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate == default || endDate == default || startDate > endDate)
            {
                return BadRequest(new { success = false, message = "Invalid date range." });
            }

            var reservations = await _unitOfWork.Reservation.GetAllAsync(
                r => r.StartDate >= startDate && r.EndDate <= endDate,
                includes: "Guest.User,Rv,Lot.LotType"
            );

            var sorted = reservations.OrderBy(r => r.Status).ToList();

            return Ok(new { success = true, data = sorted });
        }
    }
}
