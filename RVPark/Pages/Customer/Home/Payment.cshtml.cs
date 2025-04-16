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

        public Reservation Reservation { get; set; }
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
        public int GuestId { get; set; }

        public async Task<IActionResult> OnGetAsync(
        string guestFirstName, string guestLastName, string licensePlate, string make,
        string model, string rvDescription, int length,
        int numberOfAdults, int numberOfPets, string specialRequests,
        DateTime startDate, DateTime endDate, int duration, int id, int guestId)
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
            GuestId = guestId;

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

            Reservation.Duration = (Reservation.EndDate - Reservation.StartDate).Days;

            if (Reservation.Duration <= 0)
            {
                ModelState.AddModelError("", "End date must be after start date.");
                return Page();
            }

            if (Reservation.ReservationId == 0)
            {
                var user = new User
                {
                    FirstName = GuestFirstName,
                    LastName = GuestLastName,
                    Email = "placeholder@email.com",
                    Phone = "000-000-0000",
                    IsActive = true
                };

                var guest = new Guest { User = user, DodId = 0 };
                _unitOfWork.Guest.Add(guest);
                await _unitOfWork.CommitAsync();

                var rv = new RV
                {
                    Guest = guest,
                    Length = (int)Length,
                    Make = "Unknown",
                    Model = "Unknown",
                    LicensePlate = "TEMP",
                    Description = "User Input"
                };
                _unitOfWork.RV.Add(rv);
                await _unitOfWork.CommitAsync();

                Reservation.GuestId = guest.GuestID;
                Reservation.RvId = rv.RvID;
            }
            else
            {
                var existingReservation = await _unitOfWork.Reservation.GetAsync(
                    r => r.ReservationId == Reservation.ReservationId,
                    includes: "Guest.User,Rv"
                );

                if (existingReservation == null)
                {
                    ModelState.AddModelError("", "Reservation not found.");
                    return Page();
                }

                Reservation.GuestId = existingReservation.GuestId;
                Reservation.RvId = existingReservation.RvId;

                existingReservation.Guest.User.FirstName = GuestFirstName;
                existingReservation.Guest.User.LastName = GuestLastName;
                existingReservation.Rv.Length = (int)Length;

                _unitOfWork.User.Update(existingReservation.Guest.User);
                _unitOfWork.RV.Update(existingReservation.Rv);
                await _unitOfWork.CommitAsync();
            }

            var lots = await _unitOfWork.Lot.GetAllAsync();
            var reservations = await _unitOfWork.Reservation.GetAllAsync();

            var availableLots = lots
                .Where(l => l.IsAvailable && (decimal)l.Length >= Length)
                .Where(lot =>
                    !reservations.Any(r =>
                        r.LotId == lot.Id &&
                        r.ReservationId != Reservation.ReservationId &&
                        (
                            (Reservation.StartDate >= r.StartDate && Reservation.StartDate < r.EndDate) ||
                            (Reservation.EndDate > r.StartDate && Reservation.EndDate <= r.EndDate) ||
                            (Reservation.StartDate <= r.StartDate && Reservation.EndDate >= r.EndDate)
                        )
                    )
                )
                .OrderBy(l => l.Length)
                .ToList();

            var selectedLot = availableLots.FirstOrDefault();

            if (selectedLot == null)
            {
                ModelState.AddModelError("", "No available lot found for the trailer size and date range.");
                return Page();
            }

            Reservation.LotId = selectedLot.Id;

            if (!ModelState.IsValid)
                return Page();

            try
            {
                if (Reservation.ReservationId == 0)
                    _unitOfWork.Reservation.Add(Reservation);
                else
                    _unitOfWork.Reservation.Update(Reservation);

                await _unitOfWork.CommitAsync();
                TempData["Success"] = "Reservation saved successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving reservation: {ex.Message}");
                return Page();
            }

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
