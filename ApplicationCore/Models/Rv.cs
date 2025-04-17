using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ApplicationCore.Models
{
    [Table("RVs")]
    public class RV
    {
        [Key]
        public int RvId { get; set; }

        [Required]
        public int GuestId { get; set; }

        [ForeignKey(nameof(GuestId))]
        [ValidateNever]
        [Required]
        public Guest Guest { get; set; } = null!;

        [Required(ErrorMessage = "License Plate is required.")]
        [StringLength(100)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Make is required.")]
        [StringLength(100)]
        public string Make { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Length must be between 1 and 100.")]
        public int Length { get; set; } = 1;

        public bool IsArchived { get; set; } = false;

        public ICollection<Reservation> Reservations { get; set; }
            = new List<Reservation>();

        public static RV? GetGuestRV(IEnumerable<RV> rvList, int guestId)
            => rvList.FirstOrDefault(rv => rv.GuestId == guestId);
    }
}
