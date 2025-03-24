using ApplicationCore.Models;
using ApplicationCore.Models.ApplicationCore.Models;
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
        public Employee Employee { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                Employee = new Employee();
            }
            else
            {
                Employee = _unitOfWork.Employee.Get(e => e.EmployeeID == id);
                if (Employee == null)
                {
                    return NotFound();
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Employee.EmployeeID == 0)
            {
                _unitOfWork.Employee.Add(Employee);
            }
            else
            {
                _unitOfWork.Employee.Update(Employee);
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index");
        }
    }
}
