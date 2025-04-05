using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.ViewModels
{
 
    public class EmployeeViewModel
    {
  
        public int EmployeeID { get; set; }
 
        public int UserID { get; set; }
 
        [Required]
        public string FirstName { get; set; } = string.Empty;
 
        [Required]
        public string LastName { get; set; } = string.Empty;
 
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
 
        [Phone]
        public string Phone { get; set; } = string.Empty;
 
        [Required]
        public string Role { get; set; } = "Admin";
    }
}
