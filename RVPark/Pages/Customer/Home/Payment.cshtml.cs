using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Stripe;

namespace RVPark.Pages.Customer.Home
{
    public class PaymentModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly StripeSettings _stripeSettings;

        public PaymentModel(UnitOfWork unitOfWork, IOptions<StripeSettings> stripeOptions)
        {
            _unitOfWork = unitOfWork;
            _stripeSettings = stripeOptions.Value;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        [BindProperty(SupportsGet = true)]
        public string GuestFirstName { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string GuestLastName { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string LicensePlate { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string Make { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string Model { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string RvDescription { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public int Length { get; set; }
        [BindProperty(SupportsGet = true)]
        public int NumberOfAdults { get; set; }
        [BindProperty(SupportsGet = true)]
        public int NumberOfPets { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Duration { get; set; }

        public Lot SelectedLot { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public string StripePublishableKey { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => l.Id == Id,
                includes: "LotType");
            if (SelectedLot == null)
                return NotFound();

            var rate = SelectedLot.LotType?.Rate ?? 0m;
            var subtotal = rate * Duration;
            var tax = subtotal * 0.08m;
            TotalAmount = subtotal + tax;

            StripePublishableKey = _stripeSettings.PublishableKey;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string stripeToken)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                l => l.Id == Id,
                includes: "LotType");
            if (SelectedLot == null)
                return NotFound();

            var rate = SelectedLot.LotType?.Rate ?? 0m;
            var subtotal = rate * Duration;
            var tax = subtotal * 0.08m;
            TotalAmount = subtotal + tax;

            if (string.IsNullOrEmpty(stripeToken))
                return Page();

            var paymentIntent = await new PaymentIntentService().CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)Math.Round(TotalAmount * 100),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            });

            if (paymentIntent.Status != "requires_payment_method" && paymentIntent.Status != "succeeded")
                return Page();

            var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _unitOfWork.User.GetAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
            {
                ModelState.AddModelError("", "User account not found.");
                return Page();
            }

            var guest = await _unitOfWork.Guest.GetAsync(g => g.UserId == appUser.UserId);
            if (guest == null)
            {
                guest = new Guest { UserId = appUser.UserId, DodId = 0 };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                guest = await _unitOfWork.Guest.GetAsync(g => g.UserId == appUser.UserId);
            }

            var rv = await _unitOfWork.Rv.GetAsync(r => r.LicensePlate == LicensePlate);
            if (rv == null)
            {
                rv = new Rv
                {
                    GuestId = guest.GuestId,
                    LicensePlate = LicensePlate,
                    Make = Make,
                    Model = Model,
                    Description = RvDescription,
                    Length = Length
                };
                _unitOfWork.Rv.Add(rv);
                await _unitOfWork.CommitAsync();
            }

            var reservation = new Reservation
            {
                GuestId = guest.GuestId,
                RvId = rv.RvId,
                LotId = Id,
                StartDate = StartDate,
                EndDate = EndDate,
                Duration = Duration,
                NumberOfAdults = NumberOfAdults,
                NumberOfPets = NumberOfPets,
                Status = "Pending"
            };
            _unitOfWork.Reservation.Add(reservation);
            await _unitOfWork.CommitAsync();

            return RedirectToPage("Confirmation", new
            {
                GuestName = $"{GuestFirstName} {GuestLastName}",
                CheckIn = StartDate.ToString("MM-dd-yyyy"),
                CheckOut = EndDate.ToString("MM-dd-yyyy"),
                Duration = Duration,
                LotName = SelectedLot.LotType?.Name,
                TotalPaid = TotalAmount
            });
        }

    }
}
