using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Parks
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UnitOfWork _unitOfWork;

        public IndexModel(UnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // This property exposes SuperAdminRole for Razor access
        public string SuperAdminRole => SD.SuperAdminRole;

        public List<Park> Parks { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            bool isSuperAdmin = roles.Contains(SD.SuperAdminRole);

            Parks = isSuperAdmin
                ? _unitOfWork.Park.GetAll().ToList()
                : _unitOfWork.Park.GetAll(p => !p.IsArchived).ToList();

            return Page();
        }


        public IActionResult OnPostArchive(int id)
        {
            var park = _unitOfWork.Park.Get(p => p.Id == id);
            if (park != null)
            {
                park.IsArchived = true;
                _unitOfWork.Park.Update(park);
                _unitOfWork.Commit();
            }

            return RedirectToPage();
        }

        public IActionResult OnPostUnarchive(int id)
        {
            var park = _unitOfWork.Park.Get(p => p.Id == id);
            if (park != null)
            {
                park.IsArchived = false;
                _unitOfWork.Park.Update(park);
                _unitOfWork.Commit();
            }

            return RedirectToPage();
        }
    }
}
