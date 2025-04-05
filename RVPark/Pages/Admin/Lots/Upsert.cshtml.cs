using ApplicationCore.Models;
using Infrastructure.Data;
using Magnum.FileSystem;
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

        // The Lot entity being created or updated
        [BindProperty]
        public Lot LotObject { get; set; } = new();

        // Park ID is passed via query string to filter LotTypes
        [BindProperty(SupportsGet = true)]
        public int ParkId { get; set; }

        // Dropdown list of LotTypes based on ParkId
        public IEnumerable<SelectListItem> LotTypeList { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> AvailableLots { get; set; } = new List<SelectListItem>();

        // Handles form population and ID-based editing
        public IActionResult OnGet(int? id, int? parkId)
        {
            if (id == null || id == 0)
            {
                // New lot — ensure parkId is valid
                if (parkId == null) return NotFound();

                ParkId = parkId.Value;
                LotObject = new Lot();
            }
            else
            {
                // Editing existing lot — fetch from DB
                LotObject = _unitOfWork.Lot.Get(u => u.Id == id);
                if (LotObject == null) return NotFound();

                // Retrieve associated lot type to determine ParkId
                var lotType = _unitOfWork.LotType.Get(lt => lt.Id == LotObject.LotTypeId);
                if (lotType == null) return NotFound();

                ParkId = lotType.ParkId;
            }

            // Populate LotType dropdown based on selected park
            var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == ParkId);
            LotTypeList = lotTypes.Select(lt => new SelectListItem
            {
                Text = lt.Name,
                Value = lt.Id.ToString()
            });

            return Page();
        }

        // Handles image uploads and lot record persistence
        public IActionResult OnPost()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            // If validation fails, reload LotType dropdown and return
            if (!ModelState.IsValid)
            {
                var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == ParkId);
                LotTypeList = lotTypes.Select(lt => new SelectListItem
                {
                    Text = lt.Name,
                    Value = lt.Id.ToString()
                });
                return Page();
            }

            // Ensure images directory exists
            string uploadDir = Path.Combine(webRootPath, "Images/lots/");
            if (!System.IO.Directory.Exists(uploadDir))
                System.IO.Directory.CreateDirectory(uploadDir);

            // CREATE
            if (LotObject.Id == 0)
            {
                if (files.Count > 0)
                {
                    // Save uploaded image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);
                    string fullPath = Path.Combine(uploadDir, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    files[0].CopyTo(stream);

                    LotObject.Image = $"/Images/lots/{fileName}";
                }

                _unitOfWork.Lot.Add(LotObject);
            }
            // UPDATE
            else
            {
                var objFromDb = _unitOfWork.Lot.Get(u => u.Id == LotObject.Id, true);
                if (objFromDb == null) return NotFound();

                if (files.Count > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(objFromDb.Image))
                    {
                        var oldPath = Path.Combine(webRootPath, objFromDb.Image.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Save new image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);
                    string fullPath = Path.Combine(uploadDir, fileName);

                    using var stream = new FileStream(fullPath, FileMode.Create);
                    files[0].CopyTo(stream);

                    LotObject.Image = $"/Images/lots/{fileName}";
                }
                else
                {
                    // Retain existing image
                    LotObject.Image = objFromDb.Image;
                }

                _unitOfWork.Lot.Update(LotObject);
            }

            _unitOfWork.Commit();
            return RedirectToPage("./Index", new { SelectedParkId = ParkId });
        }
    }
}
