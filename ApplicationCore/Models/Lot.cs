using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{

    public class Lot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public double Length { get; set; }

        [Required]
        public double Width { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public string? Description { get; set; }

        [Required]
        public int LotTypeId { get; set; }

        [ForeignKey("LotTypeId")]
        [ValidateNever]
        public virtual LotType LotType { get; set; } = null!;
        public string? Image { get; set; }
    }
}
