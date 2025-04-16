using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace RVPark.Pages.Customer.Home
{
    public class ConfirmationModel : PageModel
    {
        // Properties to hold the confirmation details.
        public string GuestName { get; set; } = "Guest";
        public DateTime ReservationDate { get; set; } = DateTime.Now;
        public DateTime CheckInDate { get; set; } = DateTime.Now;
        public DateTime CheckOutDate { get; set; } = DateTime.Now.AddDays(1);
        public int Duration { get; set; } = 1;
        public string LotName { get; set; } = "Standard Lot";
        public decimal TotalPaid { get; set; } = 0;

        // Optionally, you can accept query parameters to populate the model.
        public void OnGet()
        {
            // If you pass confirmation details via query string, you can parse them here.
            // Example:
            if (Request.Query.ContainsKey("GuestName"))
            {
                GuestName = Request.Query["GuestName"];
            }
            if (Request.Query.ContainsKey("CheckIn"))
            {
                DateTime.TryParse(Request.Query["CheckIn"], out var checkIn);
                CheckInDate = checkIn;
            }
            if (Request.Query.ContainsKey("CheckOut"))
            {
                DateTime.TryParse(Request.Query["CheckOut"], out var checkOut);
                CheckOutDate = checkOut;
            }
            if (Request.Query.ContainsKey("Duration"))
            {
                int.TryParse(Request.Query["Duration"], out var duration);
                Duration = duration;
            }
            if (Request.Query.ContainsKey("LotName"))
            {
                LotName = Request.Query["LotName"];
            }
            if (Request.Query.ContainsKey("TotalPaid"))
            {
                decimal.TryParse(Request.Query["TotalPaid"], out var totalPaid);
                TotalPaid = totalPaid;
            }

            // You can also set the ReservationDate to today or obtain it from your business logic.
            ReservationDate = DateTime.Now;
        }
    }
}
