using DocumentFormat.OpenXml.Bibliography;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }

        public int? TransactionId { get; set; }

        [Required]
        public int FeeTypeId { get; set; }

        [ForeignKey(nameof(FeeTypeId))]
        public FeeType FeeType { get; set; } = null!;

        public int? TriggeringPolicyId { get; set; }

        [ForeignKey(nameof(TriggeringPolicyId))]
        public Policy? TriggeringPolicy { get; set; }
        public bool IsArchived { get; set; }

        public int? ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }

        [Required]
        public decimal FeeTotal { get; set; }

        public string? Notes { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public TriggerType TriggerType { get; set; }
    }
}
