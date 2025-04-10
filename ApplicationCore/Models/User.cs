using ApplicationCore.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserID { get; set; }

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

    public virtual Guest Guest { get; set; }

    public virtual Employee Employee { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
