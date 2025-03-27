using ApplicationCore.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace RVPark.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : Controller
    {
        private readonly UnitOfWork _unitOfWork;

        public PolicyController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPolicies()
        {
            var policies = await _unitOfWork.Policy.GetAllAsync();
            return Json(new { success = true, data = policies });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            var policy = await _unitOfWork.Policy.GetAsync(p => p.Id == id);
            if (policy == null)
            {
                return NotFound(new { success = false, message = "Policy not found." });
            }
            return Json(new { success = true, data = policy });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] Policy policy)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.Policy.Add(policy);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = policy });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] Policy policy)
        {
            if (id != policy.Id)
            {
                return BadRequest(new { success = false, message = "Policy ID mismatch." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data." });
            }
            _unitOfWork.Policy.Update(policy);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, data = policy });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var policy = await _unitOfWork.Policy.GetAsync(p => p.Id == id);
            if (policy == null)
            {
                return NotFound(new { success = false, message = "Policy not found." });
            }
            _unitOfWork.Policy.Delete(policy);
            await _unitOfWork.CommitAsync();
            return Json(new { success = true, message = "Policy deleted successfully." });
        }
    }
}
