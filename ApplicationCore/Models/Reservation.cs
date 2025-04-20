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

        public int? RvId { get; set; }

        [ForeignKey("RvId")]
        public Rv Rv { get; set; }

        public int? LotId { get; set; }

        [ForeignKey("LotId")]
        public Lot? Lot { get; set; }

        public int? Duration { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Status { get; set; }

        public string? OverrideReason { get; set; }

        public DateTime? CancellationDate { get; set; }
        public int NumberOfAdults { get; set; }
        public int NumberOfPets { get; set; }
        public decimal TotalDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BaseTotal { get; set; }
        public decimal ManualFeeTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal => BaseTotal + ManualFeeTotal + TaxTotal;


        public int LotTypeId { get; set; }

        public decimal CalculateBalanceDifference(DateTime newStartDate, DateTime newEndDate, decimal ratePerDay)
        {
            var newDuration = (newEndDate - newStartDate).Days;
            var newTotal = newDuration * ratePerDay;
            var currentTotal = Duration * ratePerDay;
            return (decimal)(newTotal - currentTotal);
        }

        public decimal CalculateTotal(decimal ratePerDay)
        {
            return (decimal)(Duration * ratePerDay);
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
    [NotMapped]
    public class CancellationRequest
    {
        public bool Override { get; set; }
        public int Percent { get; set; } = 100;
        public string? Reason { get; set; }
    }
}