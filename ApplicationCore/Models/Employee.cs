using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
  
    public class Employee
    {

        [Key]
        public int EmployeeID { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
