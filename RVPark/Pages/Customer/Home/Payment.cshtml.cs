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
        public Lot selectedLot { get; set; }
        public string guestFirstName { get; set; }
        public string guestLastName { get; set; }
        public string licensePlate { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string rvDescription { get; set; }
        public int length { get; set; }
        public int numberOfAdults { get; set; }
        public int numberOfPets { get; set; }
        public string specialRequests { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int duration { get; set; }

        public async Task<IActionResult> OnGetAsync(
        string guestFirstName, string guestLastName, string licensePlate, string make,
        string model, string rvDescription, int length,
        int numberOfAdults, int numberOfPets, string specialRequests,
        DateTime startDate, DateTime endDate, int duration, int id)
        {
            selectedLot = await _unitOfWork.Lot.GetAsync(
                   l => ((Lot)l).Id == id, includes: "LotType");


            this.guestFirstName = guestFirstName;
            this.guestLastName = guestLastName;
            this.licensePlate = licensePlate;
            this.make = make;
            this.model = model;
            this.rvDescription = rvDescription;
            this.length = length;
            this.numberOfAdults = numberOfAdults;
            this.numberOfPets = numberOfPets;
            this.specialRequests = specialRequests;
            this.startDate = startDate;
            this.endDate = endDate;
            this.duration = duration;

            return Page();
        }
    }
}
