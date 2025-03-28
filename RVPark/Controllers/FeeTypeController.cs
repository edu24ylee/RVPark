using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeTypeController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public FeeTypeController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeeTypes()
        {
            var feeTypes = await _unitOfWork.FeeType.GetAllAsync();
            return Json(new { success = true, data = feeTypes });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeeTypeById(int id)
        {
            var feeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (feeType == null)
            {
                return NotFound(new { success = false, message = "Fee Type not found." });
            }
            return Json(new { success = true, data = feeType });
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeeType([FromBody] FeeType feeType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.FeeType.Add(feeType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = feeType });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeeType(int id, [FromBody] FeeType feeType)
        {
            if (id != feeType.Id)
            {
                return BadRequest(new { success = false, message = "Fee Type ID mismatch." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.FeeType.Update(feeType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = feeType });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeeType(int id)
        {
            var feeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (feeType == null)
            {
                return NotFound(new { success = false, message = "Fee Type not found." });
            }
            _unitOfWork.FeeType.Delete(feeType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Fee Type deleted successfully." });
        }
    }
}
