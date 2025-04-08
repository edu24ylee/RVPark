using ApplicationCore.Models;
using ApplicationCore.ViewModels;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Employees
{
    // Handles creation and editing of Employee records
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // The view model binding to form fields in the Upsert form
        [BindProperty]
        public EmployeeViewModel EmployeeVM { get; set; } = new();

        // List of available role options shown in a dropdown (hardcoded)
        public List<string> AvailableRoles { get; set; } = new()
        {
            "Admin", "Manager", "Maintenance", "Guest"
        };

        // GET request to load form for either creation or editing
        public IActionResult OnGet(int? id)
        {
            // If no ID provided, initialize a blank form for creating a new employee
            if (id == null || id == 0)
            {
                EmployeeVM = new EmployeeViewModel();
                return Page();
            }

            // Otherwise, retrieve the existing employee and their associated user account
            var employee = _unitOfWork.Employee.Get(e => e.EmployeeID == id, includes: "User");
            if (employee == null || employee.User == null)
                return NotFound();

            // Populate the view model fields with existing values
            EmployeeVM = new EmployeeViewModel
            {
                EmployeeID = employee.EmployeeID,
                UserID = employee.User.UserID,
                FirstName = employee.User.FirstName,
                LastName = employee.User.LastName,
                Email = employee.User.Email,
                Phone = employee.User.Phone,
                Role = employee.Role
            };

            return Page();
        }

        // POST request to submit form data for creation or update
        public async Task<IActionResult> OnPostAsync()
        {
            // If form validation fails, redisplay the form with errors
            if (!ModelState.IsValid)
                return Page();

            // CREATE: New employee and user
            if (EmployeeVM.EmployeeID == 0)
            {
                var user = new User
                {
                    FirstName = EmployeeVM.FirstName.Trim(),
                    LastName = EmployeeVM.LastName.Trim(),
                    Email = EmployeeVM.Email.Trim(),
                    Phone = EmployeeVM.Phone?.Trim(),
                    IsActive = true
                };

                _unitOfWork.User.Add(user);
                await _unitOfWork.CommitAsync();

                var employee = new Employee
                {
                    UserID = user.UserID,
                    Role = EmployeeVM.Role
                };

                _unitOfWork.Employee.Add(employee);
            }
            // UPDATE: Modify existing employee and their linked user
            else
            {
                var existingEmployee = _unitOfWork.Employee.Get(e => e.EmployeeID == EmployeeVM.EmployeeID, includes: "User");
                if (existingEmployee == null || existingEmployee.User == null)
                    return NotFound();

                // Update user and employee properties
                existingEmployee.User.FirstName = EmployeeVM.FirstName;
                existingEmployee.User.LastName = EmployeeVM.LastName;
                existingEmployee.User.Email = EmployeeVM.Email;
                existingEmployee.User.Phone = EmployeeVM.Phone;
                existingEmployee.Role = EmployeeVM.Role;

                _unitOfWork.User.Update(existingEmployee.User);
                _unitOfWork.Employee.Update(existingEmployee);
            }

            // Save all changes to the database
            await _unitOfWork.CommitAsync();

            // Redirect to the index page after saving
            return RedirectToPage("./Index");
        }
    }
}
