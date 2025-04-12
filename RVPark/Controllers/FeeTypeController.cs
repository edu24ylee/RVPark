using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RVPark.Controllers
{
    [Route("api/feetypes")]
    [ApiController]
    public class FeeTypeController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public FeeTypeController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _unitOfWork.FeeType.GetAllAsync();
            return Json(new { success = true, data });
        }

        [HttpPost("archive/{id}")]
        public async Task<IActionResult> Archive(int id)
        {
            var feeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (feeType == null) return NotFound();

            feeType.IsArchived = true;
            _unitOfWork.FeeType.Update(feeType);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee Type archived." });
        }

        [HttpPost("unarchive/{id}")]
        public async Task<IActionResult> Unarchive(int id)
        {
            var feeType = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (feeType == null) return NotFound();

            feeType.IsArchived = false;
            _unitOfWork.FeeType.Update(feeType);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee Type unarchived." });
        }
    }
}
