using ApplicationCore.Models;
using ApplicationCore.Models.ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Employees
{
    public class IndexModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(UnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public IEnumerable<Employee> Employees { get; set; }
        public Dictionary<int, List<string>> EmployeeRoles { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }

        public async Task OnGetAsync(bool success = false, string message = null)
        {
            Success = success;
            Message = message;
            EmployeeRoles = new Dictionary<int, List<string>>();
            Employees = await _unitOfWork.Employee.GetAllAsync();
            foreach (var employee in Employees)
            {
                var user = await _userManager.FindByIdAsync(employee.UserID.ToString());
                var userRoles = await _userManager.GetRolesAsync(user);
                EmployeeRoles.Add(employee.EmployeeID, userRoles.ToList());
            }
        }

        public async Task<IActionResult> OnPostLockUnlockAsync(int id)
        {
            var employee = _unitOfWork.Employee.Get(e => e.EmployeeID == id);
            if (employee == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(employee.UserID.ToString());
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd == null || user.LockoutEnd <= DateTime.Now) // unlocked
            {
                user.LockoutEnd = DateTime.Now.AddYears(100);
                user.LockoutEnabled = true;
            }
            else // locked
            {
                user.LockoutEnd = DateTime.Now;
                user.LockoutEnabled = false;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Unable to update user lockout status.");
                return Page();
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage(new { success = true, message = "User lockout status updated successfully" });
        }
    }
}
