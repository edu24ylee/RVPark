using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestController : ControllerBase
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
                    fullName = g.User.FirstName + " " + g.User.LastName,
                    email = g.User.Email,
                    phone = g.User.Phone,
                    dodId = g.DodId,
                    branch = g.DodAffiliation?.Branch ?? "N/A",
                    status = g.DodAffiliation?.Status ?? "N/A",
                    rank = g.DodAffiliation?.Rank ?? "N/A"
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
    }
}
