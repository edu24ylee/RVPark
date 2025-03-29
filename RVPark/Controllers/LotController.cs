using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Infrastructure.Data;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public LotController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAllLotTypes()
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync(includes: "Park");

            var result = lotTypes.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                rate = l.Rate,
                park = new
                {
                    name = l.Park?.Name ?? "N/A"
                }
            });

            return Json(new { data = result });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetLotById(int id)
        {
            var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == id);
            if (lot == null)
            {
                return NotFound(new { success = false, message = "Lot not found." });
            }
            return Json(new { success = true, data = lot });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLot([FromBody] Lot lot)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.Lot.Add(lot);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = lot });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLot(int id, [FromBody] Lot lot)
        {
            if (id != lot.Id)
            {
                return BadRequest(new { success = false, message = "Lot ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.Lot.Update(lot);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = lot });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLot(int id)
        {
            var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == id);
            if (lot == null)
            {
                return NotFound(new { success = false, message = "Lot not found." });
            }

            _unitOfWork.Lot.Delete(lot);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Lot deleted successfully." });
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableLots()
        {
            Expression<Func<Lot, bool>> predicate = l => l.IsAvailable;
            var availableLots = await _unitOfWork.Lot.GetAllAsync(predicate);

            var projected = availableLots.Select(l => new
            {
                l.Id,
                l.Location,
                l.Length,
                l.Width,
                l.HeightLimit,
                l.IsAvailable,
                l.Description,
                LotType = new { l.LotType?.Name }
            });

            return Json(new { success = true, data = projected });
        }
        [HttpGet("bypark/{parkId}")]
        public async Task<IActionResult> GetLotsByPark(int parkId)
        {
            var lots = await _unitOfWork.Lot.GetAllAsync(
                includes: "LotType,LotType.Park");

            var filtered = lots
                .Where(l => l.LotType.ParkId == parkId)
                .Select(l => new
                {
                    l.Id,
                    l.Location,
                    l.Length,
                    l.Width,
                    l.HeightLimit,
                    l.IsAvailable,
                    l.Description,
                    lotType = new { name = l.LotType.Name },
                    park = new { name = l.LotType.Park.Name }
                });

            return Json(new { data = filtered });
        }

    }
}
