using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation Reservation { get; set; } = null!;

        [Required]
        public int GuestId { get; set; }

        [ForeignKey(nameof(GuestId))]
        public Guest Guest { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? Notes { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Method { get; set; } = "Manual"; 

        [Required]
        public string RecordedBy { get; set; } = "System";
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
