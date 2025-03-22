using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.LotTypes
{
    public class UpsertModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public LotType LotTypeObject { get; set; }
        public IEnumerable<SelectListItem> ParkList { get; set; }
        public UpsertModel(IUnitOfWork unitofWork) => _unitOfWork = unitofWork;


        public IActionResult OnGet(int? id)
        {
            LotTypeObject = new LotType();

            if (id != 0) // edit
            {

                LotTypeObject = _unitOfWork.LotType.Get(u => u.Id == id);

        
            }

            if (LotTypeObject == null)
            {
                return NotFound();
            }

            var parkList = _unitOfWork.Park.GetAll();
            ParkList = parkList.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });

            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (LotTypeObject.Id == 0) //if new
            {
                _unitOfWork.LotType.Add(LotTypeObject);
            }
            else //if existing
            {
                _unitOfWork.LotType.Update(LotTypeObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
