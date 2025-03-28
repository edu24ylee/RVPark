using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Infrastructure.Data;

namespace RVPark.Pages.Admin.Lots
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public Lot LotObject { get; set; }
        public UpsertModel(UnitOfWork unitofWork) => _unitOfWork = unitofWork;
        public IEnumerable<SelectListItem> LotTypeList { get; set; }
        public IActionResult OnGet(int? id)
        {
            LotObject = new Lot();

            if (id != 0) // edit
            {
                LotObject = _unitOfWork.Lot.Get(u => u.Id == id);
            }

            if (LotObject == null)
            {
                return NotFound();
            }

            var lotTypeList = _unitOfWork.LotType.GetAll();     
            LotTypeList = lotTypeList.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name });


            return Page();
        }
        public IActionResult OnPost()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (LotObject.Id == 0)//If new item
            {
                if (files.Count > 0)
                {
                    string filename = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"Images\lots\");
                    var extension = Path.GetExtension(files[0].FileName);
                    var fullpath = uploads + filename + extension;

                    using var fileStream = System.IO.File.Create(fullpath);
                    files[0].CopyTo(fileStream);

                    LotObject.Image = @"\Images\lots\" + filename + extension;
                }

                _unitOfWork.Lot.Add(LotObject);
            }

            else //If item already exists
            {
                var objFromDb = _unitOfWork.Lot.Get(u => u.Id == LotObject.Id, true);

                if (files.Count > 0)//Another new image was submitted
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"Images\lots\");
                    var extension = Path.GetExtension(files[0].FileName);
                    var imagePath = Path.Combine(webRootPath, objFromDb.Image.TrimStart('\\'));

                    if(System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    var fullpath = uploads + fileName + extension;

                    using var fileStream = System.IO.File.Create(fullpath);
                    files[0].CopyTo(fileStream);

                    LotObject.Image = @"\Images\lots\" + fileName + extension;
                }
                else
                {
                    LotObject.Image = objFromDb.Image; // retain the old image if no new one is uploaded
                }

                    _unitOfWork.Lot.Update(LotObject);
            }
            _unitOfWork.Commit();
            return RedirectToPage("./Index");
        }
    }
}
