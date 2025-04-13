using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Models;
using Infrastructure.Data;
using System.Threading.Tasks;
using System.Linq;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeTypesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public FeeTypesController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeeTypes()
        {
            var feeTypes = await _unitOfWork.FeeType.GetAllAsync();

            var result = feeTypes.Select(f => new
            {
                id = f.Id,
                feeTypeName = f.FeeTypeName,
                triggerType = f.TriggerType.ToString(),
                isArchived = f.IsArchived
            });

            return Json(new { data = result });
        }


        [HttpPost("archive/{id}")]
        public async Task<IActionResult> ArchiveFeeType(int id)
        {
            var type = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (type == null) return NotFound(new { success = false, message = "Fee type not found." });

            type.IsArchived = true;
            _unitOfWork.FeeType.Update(type);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee type archived." });
        }

        [HttpPost("unarchive/{id}")]
        public async Task<IActionResult> UnarchiveFeeType(int id)
        {
            var type = await _unitOfWork.FeeType.GetAsync(f => f.Id == id);
            if (type == null) return NotFound(new { success = false, message = "Fee type not found." });

            type.IsArchived = false;
            _unitOfWork.FeeType.Update(type);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee type unarchived." });
        }
    }
}
