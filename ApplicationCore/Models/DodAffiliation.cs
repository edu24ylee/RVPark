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
        public string Branch { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string Rank { get; set; }

        [Required]
        public int GuestID { get; set; }

        [ForeignKey("GuestID")]
        public Guest Guest { get; set; }
    }
}
