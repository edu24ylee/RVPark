using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class RolesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
