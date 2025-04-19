// Standard namespace for date/time and numeric types
using System;

namespace ApplicationCore.Models
{
     public class FinancialReport
    {
 
        public DateTime StartDate { get; set; }
 
        public DateTime EndDate { get; set; }

        public decimal CollectedRevenue { get; set; }
 
        public decimal AnticipatedRevenue { get; set; }
    }
}
