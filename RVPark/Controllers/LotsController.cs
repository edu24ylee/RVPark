using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class LotsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
