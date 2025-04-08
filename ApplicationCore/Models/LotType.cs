using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ApplicationCore.Models
{
 
    public class LotType
    {
 
        [Key]
        public int Id { get; set; }
 
        [Required]
        [Display(Name = "Lot Type")]
        public string Name { get; set; } = string.Empty;
 
        [Required]
        public double Rate { get; set; }
 
        public int ParkId { get; set; }
 
        [ForeignKey("ParkId")]
        [ValidateNever]
        public virtual Park Park { get; set; } = null!;
    }
}
