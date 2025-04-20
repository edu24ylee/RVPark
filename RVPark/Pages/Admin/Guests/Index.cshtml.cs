using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RVPark.Pages.Admin.Guests
{
    public class IndexModel(UnitOfWork unitOfWork) : PageModel
    {
        private readonly UnitOfWork _unitOfWork = unitOfWork;

        [BindProperty(SupportsGet = true)]
        public int? SelectedParkId { get; set; }

        public List<Guest> Guests { get; set; } = new();

        public string Message { get; set; } = "";

        public bool Success { get; set; } = false;

        public void OnGet(bool success = false, string message = "")
        {
            Success = success;
            Message = message;
        }
    }
}
