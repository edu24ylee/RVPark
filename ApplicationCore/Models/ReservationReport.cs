using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    [Table("ReservationReports")]
    public class ReservationReport
    {

        [Key]
        public int ReportId { get; set; }

        [Required]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string RVMake { get; set; } = string.Empty;


        [Required]
        public string RVModel { get; set; } = string.Empty;


        [Required]
        public string LicensePlate { get; set; } = string.Empty;


        [Required]
        public int TrailerLength { get; set; }


        [Required]
        public string LotLocation { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}