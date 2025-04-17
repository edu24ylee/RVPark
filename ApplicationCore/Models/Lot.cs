using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;

namespace ApplicationCore.Models
{
    public class Lot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public double Length { get; set; } = 0.0;

        [Required]
        public double Width { get; set; } = 0.0;

        [Required]
        public bool IsAvailable { get; set; } = false;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int LotTypeId { get; set; }

        [ForeignKey(nameof(LotTypeId))]
        [Required]
        [ValidateNever]
        public virtual LotType LotType { get; set; } = null!;

        public string ImageList { get; set; } = string.Empty;

        public string FeaturedImage { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Images =>
            string.IsNullOrWhiteSpace(ImageList)
                ? new List<string>()
                : ImageList
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .ToList();

        public bool IsFeatured { get; set; } = false;
        public bool IsArchived { get; set; } = false;

        public ICollection<Reservation> Reservations { get; set; }
            = new List<Reservation>();
    }
}
