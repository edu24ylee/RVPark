using Microsoft.AspNetCore.Mvc;

namespace RVPark.wwwroot.js
{
    public class users : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
