using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
    [Route("{organizationId}/[controller]/[action]")]
    public class UsersController : Controller
    {
        [FromRoute]
        public string OrganizationId { get; set; }
        public const string Name = "Users";      
        private readonly ILogger<UsersController> _logger;
        public UsersController(
            ILogger<UsersController> logger) 
        { 
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> OrganizationUsersIndex()
        {
            return View();
        }

        public async Task<IActionResult> POCAssignment()
        {
            return View();
        }

        public async Task<IActionResult> TeamAssignments(int timeframeId)
        {
            return View();
        }
    }
}
