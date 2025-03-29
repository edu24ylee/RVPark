using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParksController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public ParksController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllParks()
        {
            var parks = await _unitOfWork.Park.GetAllAsync();
            return Json(new { success = true, data = parks });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParkById(int id)
        {
            var park = await _unitOfWork.Park.GetAsync(p => p.Id == id);
            if (park == null)
            {
                return NotFound(new { success = false, message = "Park not found." });
            }
            return Json(new { success = true, data = park });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePark([FromBody] Park park)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.Park.Add(park);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = park });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePark(int id, [FromBody] Park park)
        {
            if (id != park.Id)
            {
                return BadRequest(new { success = false, message = "Park ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.Park.Update(park);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = park });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePark(int id)
        {
            var park = await _unitOfWork.Park.GetAsync(p => p.Id == id);
            if (park == null)
            {
                return NotFound(new { success = false, message = "Park not found." });
            }

            _unitOfWork.Park.Delete(park);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Park deleted successfully." });
        }
    }
}
