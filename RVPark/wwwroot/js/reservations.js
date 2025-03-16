using Microsoft.AspNetCore.Mvc;

namespace RVPark.wwwroot.js
{
    public class reservations : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
