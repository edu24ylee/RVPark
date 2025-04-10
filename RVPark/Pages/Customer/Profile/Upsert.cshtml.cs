using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace RVPark.Pages.Customer.Profile
{
    [Authorize]
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public UpsertModel(UnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public RV RV { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return RedirectToPage("/Account/Login");

            Email = identityUser.Email;

            var user = _unitOfWork.User.GetAll(predicate: u => u.Email == identityUser.Email).FirstOrDefault();
            if (user == null)
                return RedirectToPage("/Account/Login");

            var guest = _unitOfWork.Guest.GetAll(predicate: g => g.UserID == user.UserID).FirstOrDefault();
            if (guest == null)
                return RedirectToPage("/Account/Login");

            RV = _unitOfWork.RV.GetAll(predicate: r => r.GuestID == guest.GuestID).FirstOrDefault()
                ?? new RV { GuestID = guest.GuestID };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return RedirectToPage("/Account/Login");

            // Update Email
            if (identityUser.Email != Email)
            {
                identityUser.Email = Email;
                identityUser.UserName = Email;
                await _userManager.UpdateAsync(identityUser);
            }

            // Update RV Info
            var existingRV = _unitOfWork.RV.GetAll(predicate: r => r.RvID == RV.RvID).FirstOrDefault();
            if (existingRV != null)
            {
                existingRV.Make = RV.Make;
                existingRV.Model = RV.Model;
                existingRV.LicensePlate = RV.LicensePlate;
                existingRV.Length = RV.Length;
                _unitOfWork.RV.Update(existingRV);
            }
            else
            {
                _unitOfWork.RV.Add(RV);
            }

            _unitOfWork.Commit();
            return RedirectToPage();
        }
    }
}
