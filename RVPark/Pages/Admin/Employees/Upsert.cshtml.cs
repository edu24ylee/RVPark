using ApplicationCore.Models;
using ApplicationCore.ViewModels;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Employees
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;

        public UpsertModel(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public EmployeeViewModel EmployeeVM { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new()
        {
            "Admin", "Manager", "Maintenance", "Camp Host"
        };

        public IActionResult OnGet(int? id)
        {
            if (id == null || id == 0)
            {
                EmployeeVM = new EmployeeViewModel();
                return Page();
            }

            var employee = _unitOfWork.Employee.Get(e => e.EmployeeID == id, includes: "User");
            if (employee == null || employee.User == null)
                return NotFound();

            EmployeeVM = new EmployeeViewModel
            {
                EmployeeID = employee.EmployeeID,
                UserID = employee.User.UserId,
                FirstName = employee.User.FirstName,
                LastName = employee.User.LastName,
                Email = employee.User.Email,
                Phone = employee.User.Phone,
                Role = employee.Role
            };

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

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
                    UserId = user.UserId,
                    Role = EmployeeVM.Role
                };

                _unitOfWork.Employee.Add(employee);
            }
            else
            {
                var existingEmployee = _unitOfWork.Employee.Get(e => e.EmployeeID == EmployeeVM.EmployeeID, includes: "User");
                if (existingEmployee == null || existingEmployee.User == null)
                    return NotFound();

                existingEmployee.User.FirstName = EmployeeVM.FirstName;
                existingEmployee.User.LastName = EmployeeVM.LastName;
                existingEmployee.User.Email = EmployeeVM.Email;
                existingEmployee.User.Phone = EmployeeVM.Phone;
                existingEmployee.Role = EmployeeVM.Role;

                _unitOfWork.User.Update(existingEmployee.User);
                _unitOfWork.Employee.Update(existingEmployee);
            }

            await _unitOfWork.CommitAsync();

            return RedirectToPage("./Index");
        }
    }
}
