using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Roles
{
    public class UpsertModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpsertModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Holds the role object being created or updated
        [BindProperty]
        public IdentityRole CurrentRole { get; set; }

        // Flag to determine if we're editing an existing role or creating a new one
        [BindProperty]
        public bool IsUpdate { get; set; }

        // GET: Called when the form is loaded
        public async Task OnGetAsync(string? id)
        {
            if (id != null)
            {
                // If an ID is provided, attempt to fetch the existing role
                CurrentRole = await _roleManager.FindByIdAsync(id);
                IsUpdate = true; // Editing an existing role
            }
            else
            {
                // No ID means we're creating a new role
                CurrentRole = new IdentityRole();
                IsUpdate = false;
            }
        }

        // POST: Handles form submission
        public async Task<IActionResult> OnPostAsync()
        {
            // Normalize name for consistency (uppercase by convention)
            CurrentRole.NormalizedName = CurrentRole.Name.ToUpper();

            if (!IsUpdate)
            {
                // Create a new role
                await _roleManager.CreateAsync(CurrentRole);

                // Redirect to Index with success message
                return RedirectToPage("./Index", new { success = true, message = "Successfully Added" });
            }
            else
            {
                // Update the existing role
                await _roleManager.UpdateAsync(CurrentRole);

                // Redirect to Index with update message
                return RedirectToPage("./Index", new { success = true, message = "Update Successful" });
            }
        }
    }
}
