using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public GuestController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var guests = _unitOfWork.Guest
                .GetAll(includes: "User,DodAffiliation")
                .Select(g => new
                {
                    guestId = g.GuestId,
                    dodId = g.DodId,
                    branch = g.DodAffiliation?.Branch ?? "N/A",
                    status = g.DodAffiliation?.Status ?? "N/A",
                    rank = g.DodAffiliation?.Rank ?? "N/A",
                    user = new
                    {
                        g.User.FirstName,
                        g.User.LastName,
                        g.User.Email,
                        g.User.Phone,
                        g.User.LockOutEnd,
                        g.User.IsArchived
                    }
                });

            return Ok(new { data = guests });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var guest = _unitOfWork.Guest.Get(g => g.GuestId == id);
            if (guest == null)
                return NotFound();

            var user = _unitOfWork.User.Get(u => u.UserId == guest.UserId);
            var affiliation = _unitOfWork.DodAffiliation.Get(a => a.GuestId == guest.GuestId);

            if (affiliation != null)
                _unitOfWork.DodAffiliation.Delete(affiliation);

            _unitOfWork.Guest.Delete(guest);

            if (user != null)
                _unitOfWork.User.Delete(user);

            _unitOfWork.Commit();

            return Ok(new { success = true, message = "Deleted successfully" });
        }

        [HttpPost("archive/{id}")]
        public async Task<IActionResult> Archive(int id)
        {
            var guest = await _unitOfWork.Guest.GetAsync(g => g.GuestId == id, includes: "User");
            if (guest == null || guest.User == null)
                return NotFound(new { success = false, message = "Guest or user not found." });

            guest.User.IsArchived = true;
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Guest archived successfully." });
        }

        [HttpPost("unarchive/{id}")]
        public async Task<IActionResult> Unarchive(int id)
        {
            var guest = await _unitOfWork.Guest.GetAsync(e => e.GuestId == id, includes: "User");
            if (guest == null || guest.User == null)
                return NotFound(new { success = false, message = "Guest or user not found." });

            guest.User.IsArchived = false;
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Guest unarchived successfully." });
        }
    }
}
