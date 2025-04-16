using ApplicationCore.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using System;
using System.Threading.Tasks;

namespace RVPark.Pages.Customer.Home
{
    public class PaymentModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public PaymentModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Lot SelectedLot { get; set; }
        public string GuestFirstName { get; set; }
        public string GuestLastName { get; set; }
        public string LicensePlate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string RvDescription { get; set; }
        public int Length { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfPets { get; set; }
        public string SpecialRequests { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public decimal TotalAmount { get; set; }

        public async Task<IActionResult> OnGetAsync(
            string guestFirstName, string guestLastName, string licensePlate, string make,
            string model, string rvDescription, int length,
            int numberOfAdults, int numberOfPets, string specialRequests,
            DateTime startDate, DateTime endDate, int duration, int id)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(l => l.Id == id, includes: "LotType");

            if (SelectedLot == null)
            {
                return NotFound();
            }

            GuestFirstName = guestFirstName;
            GuestLastName = guestLastName;
            LicensePlate = licensePlate;
            Make = make;
            Model = model;
            RvDescription = rvDescription;
            Length = length;
            NumberOfAdults = numberOfAdults;
            NumberOfPets = numberOfPets;
            SpecialRequests = specialRequests;
            StartDate = startDate;
            EndDate = endDate;
            Duration = duration;

            decimal lotRate = SelectedLot.LotType?.Rate ?? 0;
            decimal subtotal = lotRate * Duration;
            decimal tax = subtotal * 0.08m;
            TotalAmount = subtotal + tax;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string stripeToken)
        {
            if (string.IsNullOrEmpty(stripeToken))
            {
                ModelState.AddModelError("", "Payment could not be processed. No token received.");
                return Page();
            }

            var paymentIntentService = new PaymentIntentService();
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt32(TotalAmount * 100),
                Currency = "usd",
                PaymentMethod = stripeToken,
                ConfirmationMethod = "manual",
                Confirm = true,
                ReturnUrl = Url.Page(
                    pageName: "/Customer/Home/Confirmation",
                    pageHandler: null,
                    values: new
                    {
                        guestName = $"{GuestFirstName} {GuestLastName}",
                        checkIn = StartDate.ToString("MM-dd-yyyy"),
                        checkOut = EndDate.ToString("MM-dd-yyyy"),
                        duration = Duration,
                        lotName = SelectedLot.LotType?.Name ?? "Unavailable",
                        totalPaid = TotalAmount
                    },
                    protocol: Request.Scheme)
            };

            PaymentIntent intent;
            try
            {
                intent = await paymentIntentService.CreateAsync(paymentIntentOptions);
            }
            catch (StripeException ex)
            {
                ModelState.AddModelError("", $"Payment error: {ex.Message}");
                return Page();
            }

            if (intent.Status != "succeeded")
            {
                ModelState.AddModelError("", "Payment not completed.");
                return Page();
            }

            _unitOfWork.Commit();

            return RedirectToPage("/Customer/Home/Confirmation", new
            {
                guestName = $"{GuestFirstName} {GuestLastName}",
                checkIn = StartDate.ToString("MM-dd-yyyy"),
                checkOut = EndDate.ToString("MM-dd-yyyy"),
                duration = Duration,
                lotName = SelectedLot.LotType?.Name ?? "Unavailable",
                totalPaid = TotalAmount
            });
        }
    }
}
