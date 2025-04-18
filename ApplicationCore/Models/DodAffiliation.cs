using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class DodAffiliation
    {
        [Key]
        public int DodAffiliationId { get; set; }

        [Required]
        [Display(Name = "Military Branch")]
        public string Branch { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Rank { get; set; } = string.Empty;

        [Required]
        public int GuestId { get; set; }

        [ForeignKey(nameof(GuestId))]
        [Required]
        public Guest Guest { get; set; } = null!;
    }
}
