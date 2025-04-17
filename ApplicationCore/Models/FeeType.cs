using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class FeeType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FeeTypeName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public TriggerType TriggerType { get; set; } = TriggerType.Manual;

        public string TriggerRuleJson { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        public ICollection<Fee> Fees { get; set; } = new List<Fee>();
    }

    public enum TriggerType
    {
        Manual = 0,
        Triggered = 1
    }

    public class CancellationFeeRule
    {
        public int DaysBefore { get; set; } = 0;
        public double PenaltyPercent { get; set; } = 0.0;
    }
}
