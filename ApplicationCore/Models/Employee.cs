using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [Required]
        public virtual User User { get; set; } = null!;

        public bool IsArchived { get; set; } = false;
    }
}
