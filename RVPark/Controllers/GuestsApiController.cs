using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers.Api
{
    [Route("api/guests")]
    [ApiController]
    public class GuestsApiController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;

        public GuestsApiController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{guestId}/rv")]
        public async Task<IActionResult> GetOrCreateRv(int guestId)
        {
            var guest = await _unitOfWork.Guest.GetAsync(g => g.GuestId == guestId, includes: "Rvs");

            if (guest == null)
                return NotFound(new { message = "Guest not found." });

            var rv = guest.Rvs?.FirstOrDefault();

            if (rv == null)
            {
    
                rv = new Rv
                {
                    GuestId = guest.GuestId,
                    Make = "Unknown",
                    Model = "Unknown",
                    LicensePlate = "TEMP",
                    Length = 0,
                    Description = "Auto-created RV"
                };

                _unitOfWork.Rv.Add(rv);
                await _unitOfWork.CommitAsync();
            }

            return Ok(new
            {
                rvId = rv.RvId,
                length = rv.Length
            });
        }
    }
}
