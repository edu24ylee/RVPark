using Microsoft.AspNetCore.Mvc;

namespace RVPark.wwwroot.js
{
    public class employees : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
