using ApplicationCore.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Azure.Documents;

namespace RVPark.Pages.Customer.Home
{
    public class PaymentModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public PaymentModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public Reservation Reservation { get; set; }
        [BindProperty]
        public Lot SelectedLot { get; set; }
        [BindProperty]
        public string GuestFirstName { get; set; }
        [BindProperty]
        public string GuestLastName { get; set; }
        [BindProperty]
        public string LicensePlate { get; set; }
        [BindProperty]
        public string Make { get; set; }
        [BindProperty]
        public string Model { get; set; }
        [BindProperty]
        public string RvDescription { get; set; } = "";
        [BindProperty]
        public int Length { get; set; }
        [BindProperty]
        public int NumberOfAdults { get; set; }
        [BindProperty]
        public int NumberOfPets { get; set; }
        [BindProperty]
        public string SpecialRequests { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }
        [BindProperty]
        public int Duration { get; set; }
        [BindProperty]
        public decimal TotalAmount { get; set; }
        [BindProperty]
        public int GuestId { get; set; }

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
            //if (string.IsNullOrEmpty(stripeToken))
            //{
            //    ModelState.AddModelError("", "Payment could not be processed. No token received.");
            //    return Page();
            //}
            this.Reservation = new Reservation
            {
                StartDate = StartDate,
                EndDate = EndDate,
                Duration = Duration,
                Status = "Pending",
                NumberOfAdults = NumberOfAdults,
                NumberOfPets = NumberOfPets,
                SpecialRequests = SpecialRequests,
                LotId = SelectedLot.Id,
            };
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claims = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var userId = claims.Value; 

                // Retrieve the corresponding User entity from the database
                var user = await _unitOfWork.User.GetAsync(u => u.IdentityUserId == userId);
                var guest = new Guest { User = user, DodId = 0 };

                var rv = new RV
                {
                    Guest = guest,
                    LicensePlate = LicensePlate,
                    Make = Make,
                    Model = Model,
                    Description = RvDescription,
                    Length = Length
                };
                _unitOfWork.RV.Add(rv);
                await _unitOfWork.CommitAsync();

                Reservation.GuestId = guest.GuestID;
                Reservation.RvId = rv.RvID;

                if (user != null)
                {
                    GuestId = user.UserID; // Map the UserID to GuestId for reservation purposes
                }
            }

            try
            {
                if (Reservation.ReservationId == 0)
                    _unitOfWork.Reservation.Add(Reservation);
                else
                    _unitOfWork.Reservation.Update(Reservation);

                await _unitOfWork.CommitAsync();
                TempData["Success"] = "Reservation saved successfully!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving reservation: {ex.Message}");
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
