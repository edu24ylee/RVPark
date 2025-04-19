using ApplicationCore.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Phone]
    public string Phone { get; set; }

    public bool IsActive { get; set; }
    public DateTime? LockOutEnd { get; set; }
    public string IdentityUserId { get; set; }
    public bool IsArchived { get; set; } = false;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
