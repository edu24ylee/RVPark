using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RVPark.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController(UnitOfWork unitOfWork) : Controller
{
    private readonly UnitOfWork _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _unitOfWork.Employee.GetAllAsync(includes: "User");

        var result = employees.Select(e => new
        {
            e.EmployeeID,
            e.Role,
            user = new
            {
                e.User.FirstName,
                e.User.LastName,
                e.User.Email,
                e.User.Phone,
                e.User.IsActive,
                e.User.LockOutEnd
            }
        });

        return Json(new { data = result });
    }

    [HttpPost("lockunlock/{id}")]
    public async Task<IActionResult> LockUnlock(int id)
    {
        var employee = await _unitOfWork.Employee.GetAsync(e => e.EmployeeID == id, includes: "User");
        if (employee == null) return NotFound(new { success = false, message = "Employee not found." });

        var user = employee.User;
        if (user == null) return NotFound(new { success = false, message = "User not found." });

        user.LockOutEnd = (user.LockOutEnd == null || user.LockOutEnd <= DateTime.Now)
            ? DateTime.Now.AddYears(100)
            : DateTime.Now;

        await _unitOfWork.CommitAsync();
        return Json(new { success = true, message = "Lock status updated." });
    }
}
