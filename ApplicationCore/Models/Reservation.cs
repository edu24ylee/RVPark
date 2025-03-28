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

        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }

        [Required]
        public int RvId { get; set; }

        [ForeignKey("RvId")]
        public RV Rv { get; set; }


        [Required]
        public int LotId { get; set; }

        [ForeignKey("LotId")]
        public Lot Lot { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; }

        public string? OverrideReason { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }

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