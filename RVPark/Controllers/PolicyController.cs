using ApplicationCore.Models;
using Infrastructure.Data;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
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

            var result = policies.Select(p => new
            {
                id = p.Id,
                policyName = p.PolicyName,
                policyDescription = p.PolicyDescription,
                isArchived = p.IsArchived 
            });

            return Json(new { data = result });
        }
        [HttpPost("archive/{id}")]
        public async Task<IActionResult> ArchivePolicy(int id)
        {
            var policy = await _unitOfWork.Policy.GetAsync(p => p.Id == id);
            if (policy == null)
                return Json(new { success = false, message = "Policy not found." });

            policy.IsArchived = true;
            _unitOfWork.Policy.Update(policy);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Policy archived." });
        }

        [HttpPost("unarchive/{id}")]
        [Authorize(Roles = SD.SuperAdminRole)]
        public async Task<IActionResult> UnarchivePolicy(int id)
        {
            var policy = await _unitOfWork.Policy.GetAsync(p => p.Id == id);
            if (policy == null)
                return Json(new { success = false, message = "Policy not found." });

            policy.IsArchived = false;
            _unitOfWork.Policy.Update(policy);
            await _unitOfWork.CommitAsync();

            return Json(new { success = true, message = "Policy unarchived." });
        }


    }
}
