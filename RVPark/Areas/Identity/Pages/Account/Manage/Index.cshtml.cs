using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UserManager<IdentityUser> userManager, UnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public RV? RV { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return RedirectToPage("/Account/Login");

            Email = identityUser.Email ?? "";
            PhoneNumber = await _userManager.GetPhoneNumberAsync(identityUser) ?? "";

            var appUser = _unitOfWork.User.Get(u => u.Email == Email);
            if (appUser != null)
            {
                var guest = _unitOfWork.Guest.Get(g => g.UserId == appUser.UserId);
                if (guest != null)
                {
                    RV = _unitOfWork.RV.Get(r => r.GuestId == guest.GuestId, trackChanges: true);
                }
            }

            return Page();
        }
    }
}
