using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Employees
{
    // Handles the Employee Index page, showing all employees
    public class IndexModel(UnitOfWork unitOfWork) : PageModel
    {
        private readonly UnitOfWork _unitOfWork = unitOfWork;

        // Optional: used to filter results by park, passed via query string
        [BindProperty(SupportsGet = true)]
        public int? SelectedParkId { get; set; }

        // List to store retrieved employees and pass to the view
        public List<Employee> Employees { get; set; } = new();

        // Used to display a message in the UI (success or failure feedback)
        public string Message { get; set; } = "";

        // Indicates whether the previous operation was successful (used for UI alerts)
        public bool Success { get; set; } = false;

        // Executed during GET requests to the Index page
        public void OnGet(bool success = false, string message = "")
        {
            Success = success;
            Message = message;

            // Employee data isn't fetched here directly — likely handled in the .cshtml or via JS
        }
    }
}
