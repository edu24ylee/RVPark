using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Roles
{
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Collection of roles to be displayed in the UI
        public IEnumerable<IdentityRole> RolesObj { get; set; }

        // Indicates whether a create or update operation succeeded
        public bool Success { get; set; }

        // Message to display on the page (e.g., "Update Successful")
        public string Message { get; set; }

        // Called when the page is loaded
        public async Task OnGetAsync(bool success = false, string message = null)
        {
            // Receive status and message from redirection (after role create/update)
            Success = success;
            Message = message;

            // Retrieve all roles from the system (IQueryable → IEnumerable)
            RolesObj = _roleManager.Roles;
        }
    }
}
