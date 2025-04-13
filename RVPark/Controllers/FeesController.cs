using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Models;
using Infrastructure.Data;
using System.Threading.Tasks;
using System.Linq;

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
            var fees = await _unitOfWork.Fee.GetAllAsync(null, null, "FeeType,TriggeringPolicy");

            var result = fees.Select(f => new
            {
                id = f.Id,
                feeType = new { feeTypeName = f.FeeType?.FeeTypeName ?? "N/A" },
                triggeringPolicy = new { policyName = f.TriggeringPolicy?.PolicyName ?? "N/A" },
                triggerType = f.TriggerType,
                feeTotal = f.FeeTotal,
                isArchived = f.IsArchived
            });

            return Json(new { success = true, data = result });
        }


        [HttpPost("archive/{id}")]
        public async Task<IActionResult> ArchiveFee(int id)
        {
            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == id);
            if (fee == null) return NotFound(new { success = false, message = "Fee not found." });

            fee.IsArchived = true;
            _unitOfWork.Fee.Update(fee);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee archived." });
        }

        [HttpPost("unarchive/{id}")]
        public async Task<IActionResult> UnarchiveFee(int id)
        {
            var fee = await _unitOfWork.Fee.GetAsync(f => f.Id == id);
            if (fee == null) return NotFound(new { success = false, message = "Fee not found." });

            fee.IsArchived = false;
            _unitOfWork.Fee.Update(fee);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Fee unarchived." });
        }
    }
}
