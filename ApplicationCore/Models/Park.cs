using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Models
{
    public class Park
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Park Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Zipcode { get; set; } = string.Empty;

        public virtual ICollection<LotType> LotTypes { get; set; } = new List<LotType>();
    }
}
