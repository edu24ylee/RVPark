using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RVPark.Pages.Admin.Lots
{
    public class UpsertModel : PageModel
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UpsertModel(UnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        
        [BindProperty]
        public Lot LotObject { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ParkId { get; set; }

        public IEnumerable<SelectListItem> LotTypeList { get; set; } = new List<SelectListItem>();

        public IActionResult OnGet(int? id, int? parkId)
        {
            if (id == null || id == 0)
            {
                // Creating new
                if (parkId == null)
                    return NotFound();

                ParkId = parkId.Value;
                LotObject = new Lot();
            }
            else
            {
                // Editing existing
                LotObject = _unitOfWork.Lot.Get(u => u.Id == id);
                if (LotObject == null)
                    return NotFound();

                var lotType = _unitOfWork.LotType.Get(lt => lt.Id == LotObject.LotTypeId);
                if (lotType == null)
                    return NotFound();

                ParkId = lotType.ParkId;
            }

            // Filter LotTypes based on the current Park
            var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == ParkId);
            LotTypeList = lotTypes.Select(lt => new SelectListItem
            {
                Text = lt.Name,
                Value = lt.Id.ToString()
            });

            return Page();
        }

        public IActionResult OnPost()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            
            if (!ModelState.IsValid)
            {
                // Repopulate LotTypeList if validation fails
                var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == ParkId);
                LotTypeList = lotTypes.Select(lt => new SelectListItem
                {
                    Text = lt.Name,
                    Value = lt.Id.ToString()
                });

                return Page();
            }

            if (LotObject.Id == 0)
            {
                // Handle image upload
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"Images\lots\");
                    var extension = Path.GetExtension(files[0].FileName);
                    var fullpath = uploads + fileName + extension;

                    using var fileStream = System.IO.File.Create(fullpath);
                    files[0].CopyTo(fileStream);

                    LotObject.Image = @"\Images\lots\" + fileName + extension;
                }

                _unitOfWork.Lot.Add(LotObject);
            }

            else //If item already exists
            {
                var objFromDb = _unitOfWork.Lot.Get(u => u.Id == LotObject.Id, true);
                if (objFromDb == null)
                    return NotFound();

                if (files.Count > 0)
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
                    LotObject.Image = objFromDb.Image;
                }

                _unitOfWork.Lot.Update(LotObject);
            }

            _unitOfWork.Commit();
            return RedirectToPage("./Index", new { SelectedParkId = ParkId });
        }
    }
}
