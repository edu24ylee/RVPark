using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class EmployeesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
