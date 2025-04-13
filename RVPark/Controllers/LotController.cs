using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LotController : Controller
{
    private readonly UnitOfWork _unitOfWork;

    public LotController(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet("bypark/{parkId}")]
    public async Task<IActionResult> GetByPark(int parkId)
    {
        var lots = await _unitOfWork.Lot.GetAllAsync(
            l => l.LotType.ParkId == parkId,
            includes: "LotType.Park"
        );

        var result = lots.Select(l => new
        {
            id = l.Id,
            location = l.Location,
            width = l.Width,
            length = l.Length,
            isAvailable = l.IsAvailable,
            description = l.Description,
            image = string.Join(",", (l.ImageList ?? l.Image ?? "")
                .Split(',', ';')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.StartsWith("/Images/lots/") ? s : $"/Images/lots/{s}")),
            featuredImage = string.IsNullOrEmpty(l.FeaturedImage)
        ? ""
        : (l.FeaturedImage.StartsWith("/Images/lots/") ? l.FeaturedImage : $"/Images/lots/{l.FeaturedImage}"),
            lotType = new { name = l.LotType.Name },
            park = new { name = l.LotType.Park.Name },
            isArchived = l.IsArchived,
            isFeatured = l.IsFeatured
        });


        return Json(new { data = result });
    }



    [HttpPost("archive/{id}")]
    public async Task<IActionResult> Archive(int id)
    {
        var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == id);
        if (lot == null)
            return Json(new { success = false, message = "Lot not found." });

        lot.IsArchived = true;
        _unitOfWork.Lot.Update(lot);
        await _unitOfWork.CommitAsync();

        return Json(new { success = true, message = "Lot archived successfully." });
    }

    [HttpPost("unarchive/{id}")]
    public async Task<IActionResult> Unarchive(int id)
    {
        var lot = await _unitOfWork.Lot.GetAsync(l => l.Id == id);
        if (lot == null)
            return Json(new { success = false, message = "Lot not found." });

        lot.IsArchived = false;
        _unitOfWork.Lot.Update(lot);
        await _unitOfWork.CommitAsync();

        return Json(new { success = true, message = "Lot unarchived successfully." });
    }
    [HttpPost("feature/{id}")]
    public async Task<IActionResult> FeatureLot(int id)
    {
        var lots = await _unitOfWork.Lot.GetAllAsync();

        foreach (var lot in lots)
            lot.IsFeatured = lot.Id == id;

        _unitOfWork.Lot.UpdateRange(lots);
        await _unitOfWork.CommitAsync();

        return Json(new { success = true, message = "Featured lot updated." });
    }
}
