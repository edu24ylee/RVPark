using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }
        public int? TransactionId { get; set; }
        [Required]
        public int FeeTypeId { get; set; }
        [ForeignKey("FeeTypeId")]
        public FeeType FeeType { get; set; }
        public int? TriggeringPolicyId { get; set; }
        [ForeignKey("TriggeringPolicyId")]
        public Policy TriggeringPolicy { get; set; }
        [Required]
        public decimal FeeTotal { get; set; }
    }
}
