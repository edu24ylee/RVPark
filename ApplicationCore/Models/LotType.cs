using System;
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
        public decimal Rate { get; set; } = 0.00m;

        [Required]
        public int ParkId { get; set; }

        [ForeignKey(nameof(ParkId))]
        [ValidateNever]
        [Required]
        public virtual Park Park { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; } = DateTime.Today;

        public bool IsArchived { get; set; } = false;

        public ICollection<Lot> Lots { get; set; } = new List<Lot>();
    }
}
