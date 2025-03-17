using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
