using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Fee
    {
        public int Id { get; set; }

        public int FeeTypeId { get; set; }
        public FeeType? FeeType { get; set; }

        [Required]
        public decimal FeeTotal { get; set; }

        public string? Notes { get; set; }

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public TriggerType TriggerType { get; set; }

        public bool IsArchived { get; set; }
        public int? ReservationId { get; set; }

        [ForeignKey(nameof(ReservationId))]
        public Reservation? Reservation { get; set; }
    }

    public class ManualFeeOptionViewModel
    {
        public int Id { get; set; }
        public string FeeTypeName { get; set; }
        public decimal FeeTotal { get; set; }
    }
}