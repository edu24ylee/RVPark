using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    [NotMapped]
    public class FinancialReport
    {
        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.UtcNow;

        [Required]
        public decimal CollectedRevenue { get; set; } = 0m;

        [Required]
        public decimal AnticipatedRevenue { get; set; } = 0m;
    }
}
