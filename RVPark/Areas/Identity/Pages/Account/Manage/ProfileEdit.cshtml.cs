using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class ProfileEditModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UnitOfWork _unitOfWork;

    public ProfileEditModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, UnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public string Username { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new InputModel(); // avoid null warnings

    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "RV Make")]
        public string Make { get; set; } = string.Empty;

        [Display(Name = "RV Model")]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [Display(Name = "Length (ft)")]
        public int Length { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var identityUser = await _userManager.GetUserAsync(User);
        if (identityUser == null)
            return RedirectToPage("/Account/Login");

        Username = identityUser.UserName;
        var phoneNumber = await _userManager.GetPhoneNumberAsync(identityUser);
        Input.PhoneNumber = phoneNumber ?? string.Empty;

        var appUser = _unitOfWork.User.Get(u => u.Email == identityUser.Email);
        if (appUser != null)
        {
            var guest = _unitOfWork.Guest.Get(g => g.UserId == appUser.UserId);
            if (guest != null)
            {
                var rv = _unitOfWork.Rv.Get(r => r.GuestId == guest.GuestId);
                if (rv != null)
                {
                    Input.Make = rv.Make;
                    Input.Model = rv.Model;
                    Input.LicensePlate = rv.LicensePlate;
                    Input.Length = rv.Length;
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var identityUser = await _userManager.GetUserAsync(User);
        if (identityUser == null)
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
            return Page();

        var currentPhoneNumber = await _userManager.GetPhoneNumberAsync(identityUser);
        if (Input.PhoneNumber != currentPhoneNumber)
        {
            var result = await _userManager.SetPhoneNumberAsync(identityUser, Input.PhoneNumber);
            if (!result.Succeeded)
            {
                StatusMessage = "Unexpected error when setting phone number.";
                return RedirectToPage();
            }
        }

        var appUser = _unitOfWork.User.Get(u => u.Email.ToLower() == identityUser.Email.ToLower());
        if (appUser == null)
        {
            Console.WriteLine(" appUser not found.");
        }
        else
        {
            Console.WriteLine($"appUser found: {appUser.UserId}");
            var guest = _unitOfWork.Guest.Get(g => g.UserId == appUser.UserId);
            if (guest == null)
            {
                Console.WriteLine("guest not found.");
            }
            else
            {
                Console.WriteLine($"guest found: {guest.GuestId}");
                var rv = _unitOfWork.Rv.Get(r => r.GuestId == guest.GuestId);
                Console.WriteLine(rv == null ? "⚠️ RV not found. Will add new." : "✅ RV found. Will update.");
            }
        }


        await _signInManager.RefreshSignInAsync(identityUser);
        StatusMessage = "Profile updated successfully.";
        return RedirectToPage("Index");
    }
}
