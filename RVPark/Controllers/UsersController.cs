using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
