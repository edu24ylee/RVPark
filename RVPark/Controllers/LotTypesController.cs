using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotTypesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public LotTypesController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLotTypes()
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync();
            return Json(new { success = true, data = lotTypes });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLotTypeById(int id)
        {
            var lotType = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
            if (lotType == null)
            {
                return NotFound(new { success = false, message = "LotType not found." });
            }
            return Json(new { success = true, data = lotType });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLotType([FromBody] LotType lotType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.LotType.Add(lotType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = lotType });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLotType(int id, [FromBody] LotType lotType)
        {
            if (id != lotType.Id)
            {
                return BadRequest(new { success = false, message = "LotType ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            _unitOfWork.LotType.Update(lotType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = lotType });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLotType(int id)
        {
            var lotType = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
            if (lotType == null)
            {
                return NotFound(new { success = false, message = "LotType not found." });
            }

            _unitOfWork.LotType.Delete(lotType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "LotType deleted successfully." });
        }
    }
}
