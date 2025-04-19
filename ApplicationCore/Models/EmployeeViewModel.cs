using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.ViewModels
{
 
    public class EmployeeViewModel
    {
  
        public int EmployeeId { get; set; }
 
        public int UserId { get; set; }
 
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
        public bool IsArchived { get; set; } = false;
    }
}
