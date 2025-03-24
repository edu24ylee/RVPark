using ApplicationCore.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Phone]
        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public virtual Guest Guest { get; set; }
        public virtual Employee Employee { get; set; }


        public Guest GetGuest()
        {
            return Guest;
        }

        public Employee GetEmployee()
        {
            return Employee;
        }

        public User() { }
    }
}