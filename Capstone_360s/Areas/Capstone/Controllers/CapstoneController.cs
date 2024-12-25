using Capstone_360s.Controllers;
using Capstone_360s.Services.Organizations;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capstone_360s.Interfaces.IService;

namespace Capstone_360s.Areas.Capstone.Controllers
{
    [Area("Capstone")]
    public class CapstoneController : BaseController
    {
        public const string Name = "Capstone";
        private readonly CapstoneService _capstoneService;
        private readonly ILogger<CapstoneController> _logger;

        public CapstoneController(
            CapstoneService capstoneService,
            ILogger<CapstoneController> logger
        )
        {
            _capstoneService = capstoneService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> UploadRoster(IFormFile roster, DateTime filterDate, int roundId, [FromQuery] int timeframeId, [FromQuery] string organizationId)
        {
            if(roster == null || filterDate == DateTime.MinValue || roundId == 0 || timeframeId == 0 || string.IsNullOrEmpty(organizationId))
            {
                throw new ArgumentNullException("One of the parameters was empty.");
            }

            await _capstoneService.UploadRoster(roster, filterDate, roundId, timeframeId, Guid.Parse(organizationId));

            _logger.LogInformation("Navigating to the pdf generation screen...");

            var redirectUrl = Url.Action(nameof(BaseController.GeneratePdfs), Name, new { organizationId = organizationId, timeframeId = timeframeId, roundId = roundId});

            return Json(new { redirectUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> GeneratePdfs([FromQuery] string organizationId, [FromQuery] int timeframeId, [FromQuery] int roundId)
        {
            if(roundId == 0 || timeframeId == 0 || string.IsNullOrEmpty(organizationId))
            {
                throw new ArgumentNullException("One of the parameters was empty.");
            }

            try {
                await _capstoneService.CreatePdfs(Guid.Parse(organizationId), timeframeId, roundId);
            } 
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }


            _logger.LogInformation("Done! Navigating to the projects index...");

            var redirectUrl = Url.Action(nameof(UploadProcessController.ProjectsIndex), UploadProcessController.Name, new { organizationId = organizationId, timeframeId = timeframeId });

            return Json(new { redirectUrl });
        }
    }
}
