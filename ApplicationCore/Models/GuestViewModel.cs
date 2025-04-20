using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.ViewModels
{
    public class GuestViewModel
    {
        public int GuestId { get; set; }
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

        public int? DodId { get; set; }

        public string DodBranch { get; set; } = string.Empty;
        public string DodStatus { get; set; } = string.Empty;
        public string DodRank { get; set; } = string.Empty;
    }
}
