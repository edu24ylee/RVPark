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
        public int? SelectedParkId { get; set; }

        public IEnumerable<SelectListItem> LotTypeList { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public IFormFileCollection Images { get; set; } = default!;

        [BindProperty]
        public List<string> DeleteImages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id, int? parkId)
        {
            if (id == null || id == 0)
            {
                if (parkId == null) return NotFound();
                SelectedParkId = parkId.Value;
                LotObject = new Lot();
            }
            else
            {
                LotObject = _unitOfWork.Lot.Get(u => u.Id == id);
                if (LotObject == null) return NotFound();

                var lotType = _unitOfWork.LotType.Get(lt => lt.Id == LotObject.LotTypeId);
                if (lotType == null) return NotFound();

                SelectedParkId = lotType.ParkId;
            }

            var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == SelectedParkId);
            LotTypeList = lotTypes.Select(lt => new SelectListItem
            {
                Text = lt.Name,
                Value = lt.Id.ToString()
            });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var lotTypes = _unitOfWork.LotType.GetAll(lt => lt.ParkId == SelectedParkId);
                LotTypeList = lotTypes.Select(lt => new SelectListItem
                {
                    Text = lt.Name,
                    Value = lt.Id.ToString()
                });
                return Page();
            }

            string webRootPath = _webHostEnvironment.WebRootPath;
            string uploadDir = Path.Combine(webRootPath, "Images/lots");

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var existingImages = LotObject.Image?.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new();

            if (DeleteImages.Any())
            {
                foreach (var img in DeleteImages)
                {
                    var fullPath = Path.Combine(webRootPath, img.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                existingImages = existingImages.Where(img => !DeleteImages.Contains(img)).ToList();
            }

            var newImagePaths = new List<string>();
            if (Images.Count > 0)
            {
                foreach (var file in Images)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var fullPath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    newImagePaths.Add("/Images/lots/" + fileName);
                }
            }

            var combinedImages = existingImages.Concat(newImagePaths).ToList();
            LotObject.Image = string.Join(",", combinedImages);

            if (LotObject.Id == 0)
            {
                if (string.IsNullOrEmpty(LotObject.FeaturedImage) && combinedImages.Count > 0)
                {
                    LotObject.FeaturedImage = combinedImages[0]; 
                }

                _unitOfWork.Lot.Add(LotObject);
            }
            else
            {
                var objFromDb = _unitOfWork.Lot.Get(u => u.Id == LotObject.Id);
                if (objFromDb == null) return NotFound();

                if (string.IsNullOrEmpty(LotObject.FeaturedImage) && combinedImages.Count > 0)
                {
                    LotObject.FeaturedImage = combinedImages[0];
                }

                _unitOfWork.Lot.Update(LotObject);
            }

            await _unitOfWork.CommitAsync();
            return RedirectToPage("./Index", new { selectedParkId = SelectedParkId });
        }
    }
}
