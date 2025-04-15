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

        public async Task<IActionResult> OnGetAsync(
        string guestFirstName, string guestLastName, string licensePlate, string make,
        string model, string rvDescription, int length,
        int numberOfAdults, int numberOfPets, string specialRequests,
        DateTime startDate, DateTime endDate, int duration, int id)
        {
            SelectedLot = await _unitOfWork.Lot.GetAsync(
                   l => ((Lot)l).Id == id, includes: "LotType");


            this.GuestFirstName = guestFirstName;
            this.GuestLastName = guestLastName;
            this.LicensePlate = licensePlate;
            this.Make = make;
            this.Model = model;
            this.RvDescription = rvDescription;
            this.Length = length;
            this.NumberOfAdults = numberOfAdults;
            this.NumberOfPets = numberOfPets;
            this.SpecialRequests = specialRequests;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Duration = duration;

            return Page();
        }
    }
}
