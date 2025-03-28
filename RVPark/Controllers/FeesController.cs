using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FeesController : Controller
    {
		private readonly UnitOfWork _unitOfWork;
		public FeesController(UnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

		[HttpGet]
		public IActionResult Get()
		{
			return Json(new { data = _unitOfWork.Fee.List() });
		}

		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			var objFromDb = _unitOfWork.Fee.Get(c => c.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			_unitOfWork.Fee.Delete(objFromDb);
			_unitOfWork.Commit();
			return Json(new { success = true, message = "Delete successful" });
		}
	}
}
