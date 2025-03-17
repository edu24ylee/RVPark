using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class FeesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
