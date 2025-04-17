using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int GuestId { get; set; }
        [ForeignKey(nameof(GuestId))]
        [Required]
        public Guest Guest { get; set; } = null!;

        [Required]
        public int RvId { get; set; }
        [ForeignKey(nameof(RvId))]
        [Required]
        public RV Rv { get; set; } = null!;

        [Required]
        public int LotId { get; set; }
        [ForeignKey(nameof(LotId))]
        [Required]
        public Lot Lot { get; set; } = null!;

        [Required]
        public int LotTypeId { get; set; }
        [ForeignKey(nameof(LotTypeId))]
        [Required]
        public LotType LotType { get; set; } = null!;

        [Required]
        public int Duration { get; set; } = 1;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(1);

        [Required]
        public string Status { get; set; } = "Active";

        [Required]
        public int NumberOfAdults { get; set; } = 1;

        [Required]
        public int NumberOfPets { get; set; } = 0;

        public string SpecialRequests { get; set; } = string.Empty;
        public string OverrideReason { get; set; } = string.Empty;

        public DateTime? CancellationDate { get; set; }
        public string CancellationReason { get; set; } = string.Empty;

        public decimal CalculateBalanceDifference(DateTime newStartDate, DateTime newEndDate, decimal ratePerDay)
        {
            var newDuration = (newEndDate - newStartDate).Days;
            var newTotal = newDuration * ratePerDay;
            var currentTotal = Duration * ratePerDay;
            return newTotal - currentTotal;
        }

        public decimal CalculateTotal(decimal ratePerDay)
            => Duration * ratePerDay;

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
