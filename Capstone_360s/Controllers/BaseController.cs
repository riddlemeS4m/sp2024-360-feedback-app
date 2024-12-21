using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
    public abstract class BaseController : Controller
    {

        protected BaseController()
        {
        }

        [HttpGet]
        public IActionResult UploadRoster(string organizationId, int timeframeId)
        {
            if(string.IsNullOrEmpty(organizationId) || timeframeId == 0)
            {
                return BadRequest();
            }

            return View();
        }     

        [HttpGet]   
        public IActionResult GeneratePdfs(string organizationId, int timeframeId, int roundId)
        {
            if(string.IsNullOrEmpty(organizationId) || timeframeId == 0 || roundId == 0)
            {
                return BadRequest();
            }

            return View();
        }
    }
}
