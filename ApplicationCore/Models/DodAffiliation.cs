using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class DodAffiliation
    {
        public int DodAffiliationId { get; set; }

        [Display(Name = "Military Branch")]
        public string? Branch { get; set; }

        public string? Status { get; set; }

        public string? Rank { get; set; }

        [Required]
        public int GuestId { get; set; }

        [ForeignKey("GuestId")]
        public Guest Guest { get; set; }
    }
}
