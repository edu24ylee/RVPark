using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Models
{
    public class Policy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PolicyName { get; set; } = string.Empty;

        public string PolicyDescription { get; set; } = string.Empty;

        public ICollection<Fee> Fees { get; set; } = new List<Fee>();

        public bool IsArchived { get; set; } = false;
    }
}
