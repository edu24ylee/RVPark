using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        public int GuestId { get; set; }

        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }

        public int RvId { get; set; }

        [ForeignKey("RvId")]
        public RV Rv { get; set; }

        public int LotId { get; set; }

        [ForeignKey("LotId")]
        public Lot Lot { get; set; }

        public int Duration { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfPets { get; set; }
        public string? SpecialRequests { get; set; }

        public string? OverrideReason { get; set; }

        public DateTime? CancellationDate { get; set; }

        public string? CancellationReason { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfPets { get; set; }

        public int LotTypeId { get; set; }


        public decimal CalculateBalanceDifference(DateTime newStartDate, DateTime newEndDate, decimal ratePerDay)
        {
            var newDuration = (newEndDate - newStartDate).Days;
            var newTotal = newDuration * ratePerDay;
            var currentTotal = Duration * ratePerDay;
            return newTotal - currentTotal;
        }

        public decimal CalculateTotal(decimal ratePerDay)
        {
            return Duration * ratePerDay;
        }

        public void UpdateDuration(int newDuration)
        {
            Duration = newDuration;
            EndDate = StartDate.AddDays(newDuration);
        }

        public void CancelReservation()
        {
            Status = "Cancelled";
            CancellationDate = DateTime.UtcNow;
        }
    }
}
