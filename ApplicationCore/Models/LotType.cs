using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class LotType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Lot Type")]
        public string Name { get; set; }
        [Required]
        public double Rate { get; set; }
        [Required]
       // public DateOnly StartDate { get; set; }
        //[Required]
        //public DateOnly EndDate { get; set; }
        //[Required]
        public int ParkId { get; set; }
        [ForeignKey("ParkId")]
        public virtual Park Park { get; set; }
    }
}
