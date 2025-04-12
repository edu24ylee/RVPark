using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Customer.Home
{
    public class PaymentModel : PageModel
    {
        [TempData] public string firstName { get; set; }
        [TempData] public string lastName { get; set; }
        [TempData] public decimal trailerLength { get; set; }
        [TempData] public int adults { get; set; }
        [TempData] public int pets { get; set; }
        [TempData] public string specialRequests { get; set; }
        [TempData] public DateTime startDate { get; set; }
        [TempData] public DateTime endDate { get; set; }
        [TempData] public int duration { get; set; }
        [TempData] public Lot selectedLot { get; set; }

        public void OnGet()
        {
            // You can now use these values on the Payment page
        }
    }
}

