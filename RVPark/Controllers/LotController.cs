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
        private readonly IWebHostEnvironment _hostingEnv;

        public LotController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLots()
        {
            var lots = await _unitOfWork.Lot.GetAllAsync();
            return Json(new { success = true, data = lots });
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
            if(lot.Image != null)
            {
                var imgPath = Path.Combine(_hostingEnv.WebRootPath, lot.Image.TrimStart('\\'));
                if(System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }
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
            return Json(new { success = true, data = availableLots });
        }
    }
}
