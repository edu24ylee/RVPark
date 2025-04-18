using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? LockOutEnd { get; set; }

        [Required]
        public string IdentityUserId { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        public virtual Guest Guest { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
