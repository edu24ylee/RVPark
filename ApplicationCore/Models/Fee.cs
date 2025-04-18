using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FeeTypeId { get; set; }

        [ForeignKey(nameof(FeeTypeId))]
        [Required]
        public FeeType FeeType { get; set; } = null!;

        public int? TriggeringPolicyId { get; set; }

        [ForeignKey(nameof(TriggeringPolicyId))]
        public Policy? TriggeringPolicy { get; set; }

        [Required]
        public decimal FeeTotal { get; set; } = 0m;

        public string Notes { get; set; } = string.Empty;

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public TriggerType TriggerType { get; set; } = default;

        public bool IsArchived { get; set; } = false;

        public int? ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
    }
}
