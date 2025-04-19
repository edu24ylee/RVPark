#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using ApplicationCore.Models;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Infrastructure.Utilities;

namespace RVPark.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UnitOfWork _unitOfWork;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string Phone { get; set; }

            [Display(Name = "DoD ID")]
            public int? DodId { get; set; } 

            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Military Branch")]
            public string Branch { get; set; }

            public string Rank { get; set; }
            public string Status { get; set; }

            public string IdentityUserId { get; set; }

            public string SelectedRole { get; set; } 
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            if (await _userManager.FindByEmailAsync(Input.Email) != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                return Page();
            }

            // Choose password based on role
            var password = !string.IsNullOrEmpty(Input.SelectedRole) ? SD.DefaultPassword : Input.Password;

            var identityUser = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.Phone
            };

            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            var user = new User
            {
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Phone = Input.Phone,
                IsActive = true,
                IdentityUserId = identityUser.Id
            };

            _unitOfWork.User.Add(user);
            _unitOfWork.Commit();

            if (!string.IsNullOrEmpty(Input.SelectedRole) &&
                (User.IsInRole(SD.SuperAdminRole) || User.IsInRole(SD.AdminRole)))
            {
                await _userManager.AddToRoleAsync(identityUser, Input.SelectedRole);

                var employee = new Employee
                {
                    UserId = user.UserId,
                    Role = Input.SelectedRole
                };
                _unitOfWork.Employee.Add(employee);
                _unitOfWork.Commit();
            }
            else
            {
                var guest = new Guest
                {
                    UserId = user.UserId,
                    DodId = Input.DodId ?? 0,
                    DodAffiliation = new DodAffiliation
                    {
                        Branch = Input.Branch,
                        Rank = Input.Rank,
                        Status = Input.Status
                    }
                };
                _unitOfWork.Guest.Add(guest);
                _unitOfWork.Commit();
            }

            await _emailSender.SendEmailAsync(
                user.Email,
                "Welcome to Nellis AFB RV Park!",
                $@"<p>Dear {user.FullName},</p>
                <p>Welcome to the <strong>Nellis AFB RV Park</strong> family!</p>
                <p>Your account has been successfully created.</p>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='https://example.com/Customer/Reservations/Create' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;'>
                        Make a Reservation
                    </a>
                </p>
                <p>Warm regards,<br/>The Nellis AFB RV Park Team</p>"
            );

            return RedirectToPage("/Account/Login");
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");

            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
