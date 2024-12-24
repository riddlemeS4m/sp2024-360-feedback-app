using Capstone_360s.Interfaces.IService;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public abstract class BaseController : Controller
    {
        protected BaseController()
        {
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public IActionResult UploadRoster(string organizationId, int timeframeId)
        {
            if(string.IsNullOrEmpty(organizationId) || timeframeId == 0)
            {
                return BadRequest();
            }

            return View();
        }     

        [HttpGet]
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> GeneratePdfs(string organizationId, int timeframeId, int roundId)
        {
            if(string.IsNullOrEmpty(organizationId) || timeframeId == 0 || roundId == 0)
            {
                return BadRequest();
            }
            
            return View();
        }
    }
}
