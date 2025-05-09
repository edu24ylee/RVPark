﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Rv
    {
        [Key]
        public int RvId { get; set; }

        [Required]
        public int GuestId { get; set; }

        [ForeignKey("GuestId")]
        [ValidateNever]
        public Guest Guest { get; set; }

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

        [Range(1, 100, ErrorMessage = "Length must be between 1 and 100.")]
        public int Length { get; set; }

        public static Rv GetGuestRV(List<Rv> rvList, int guestId)
        {
            return rvList.FirstOrDefault(rv => rv.GuestId == guestId);
        }
    }
}
