using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotTypeController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public LotTypeController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync(includes: "Park");
            var result = lotTypes.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                rate = l.Rate,
                startDate = l.StartDate,
                endDate = l.EndDate,
                isArchived = l.IsArchived,
                parkName = l.Park?.Name ?? "N/A"
            });

            return Json(new { data = result });
        }

        [HttpGet("bypark/{parkId}")]
        public async Task<IActionResult> GetByPark(int parkId)
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync(l => l.ParkId == parkId, includes: "Park");
            var result = lotTypes.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                rate = l.Rate,
                startDate = l.StartDate,
                endDate = l.EndDate,
                isArchived = l.IsArchived,
                parkName = l.Park?.Name ?? "N/A"
            });

            return Json(new { data = result });
        }

        [HttpPost("archive/{id}")]
        public async Task<IActionResult> Archive(int id)
        {
            var lotType = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
            if (lotType == null)
                return Json(new { success = false, message = "Lot type not found." });

            lotType.IsArchived = true;
            _unitOfWork.LotType.Update(lotType);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Lot type archived." });
        }

        [HttpPost("unarchive/{id}")]
        public async Task<IActionResult> Unarchive(int id)
        {
            var lotType = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
            if (lotType == null)
                return Json(new { success = false, message = "Lot type not found." });

            lotType.IsArchived = false;
            _unitOfWork.LotType.Update(lotType);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Lot type unarchived." });
        }
    }
}
