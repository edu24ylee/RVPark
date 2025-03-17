using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class ReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
