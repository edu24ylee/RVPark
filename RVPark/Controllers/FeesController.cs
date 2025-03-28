using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Models;
using Infrastructure.Data;
using System.Threading.Tasks;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public FeesController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFees()
        {
            var fees = await _unitOfWork.Fee.GetAllAsync();
            return Json(new { success = true, data = fees });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeeById(int id)
        {
            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == id);
            if (fee == null)
            {
                return NotFound(new { success = false, message = "Fee not found." });
            }
            return Json(new { success = true, data = fee });
        }

        [HttpPost]
        public async Task<IActionResult> CreateFee([FromBody] Fee fee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.Fee.Add(fee);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = fee });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFee(int id, [FromBody] Fee fee)
        {
            if (id != fee.Id)
            {
                return BadRequest(new { success = false, message = "Fee ID mismatch." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.Fee.Update(fee);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = fee });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFee(int id)
        {
            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == id);
            if (fee == null)
            {
                return NotFound(new { success = false, message = "Fee not found." });
            }
            _unitOfWork.Fee.Delete(fee);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Fee deleted successfully." });
        }
    }
}
