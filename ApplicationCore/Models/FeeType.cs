using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Models
{
    public class FeeType
    {
        public int Id { get; set; }

        [Required]
        public string FeeTypeName { get; set; } = string.Empty;

        public string? Policy { get; set; }

        public TriggerType TriggerType { get; set; }

        public string? TriggerRuleJson { get; set; }

        public bool IsArchived { get; set; }


        public ICollection<Fee>? Fees { get; set; }
    }

    public enum TriggerType
    {
        Manual = 0,
        Triggered = 1
    }

    public class CancellationFeeRule
    {
        public int DaysBefore { get; set; }
        public double PenaltyPercent { get; set; }
    }
}