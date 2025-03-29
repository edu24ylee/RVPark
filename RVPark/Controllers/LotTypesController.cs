using ApplicationCore.Models;
using Infrastructure.Data;
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
        public async Task<IActionResult> GetAll()
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync(includes: "Park");

            var result = lotTypes.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                rate = l.Rate,
                parkName = l.Park?.Name ?? "N/A"
            });

            return Json(new { data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var lotType = await _unitOfWork.LotType.GetAsync(l => l.Id == id);
            if (lotType == null)
            {
                return Json(new { success = false, message = "Lot type not found." });
            }

            _unitOfWork.LotType.Delete(lotType);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Lot type deleted successfully." });
        }

        [HttpGet("GetByPark/{parkId}")]
        public async Task<IActionResult> GetByPark(int parkId)
        {
            var lotTypes = await _unitOfWork.LotType.GetAllAsync(
                l => l.ParkId == parkId,
                orderBy: null,
                includes: "Park"); 
            var result = lotTypes.Select(l => new
            {
                id = l.Id,
                name = l.Name,
                rate = l.Rate
            });

            return Json(new { data = result });
        }
    }
}
