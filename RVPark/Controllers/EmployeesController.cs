using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnv;

        public EmployeesController(UnitOfWork unitOfWork, IWebHostEnvironment hostingEnv)
        {
            _unitOfWork = unitOfWork;
            _hostingEnv = hostingEnv;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new { data = _unitOfWork.Employee.GetAll(null, null, "User") });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Employee.Get(e => e.EmployeeID == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Employee.Delete(objFromDb);
            _unitOfWork.Commit();
            return Json(new { success = true, message = "Delete successful" });
        }
    }
}
