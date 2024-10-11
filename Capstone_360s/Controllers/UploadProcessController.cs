using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Services.FeedbackDb;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public class UploadProcessController : Controller
    {
        private readonly IGoogleDrive _googleDriveService;
        private readonly OrganizationService _organizationService;
        private readonly TimeframeService _timeframeService;
        private readonly ProjectService _projectService;
        private readonly ILogger<UploadProcessController> _logger;
        public UploadProcessController(IGoogleDrive googleDriveService,
            OrganizationService organizationService,
            TimeframeService timeframeService,
            ILogger<UploadProcessController> logger) 
        { 
            _googleDriveService = googleDriveService;
            _organizationService = organizationService;
            _timeframeService = timeframeService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting upload process...");

            var organizations = await _organizationService.GetAllAsync();

            _logger.LogInformation("Returning organization selection view...");
            return View(organizations);
        }

        public IActionResult OrganizationCreate()
        {
            _logger.LogInformation("A new organization needs to be created...");
            _logger.LogInformation("Returning organization creation view...");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OrganizationCreate([Bind(nameof(Organization.Id),nameof(Organization.Name))] Organization organization)
        {
            _logger.LogInformation("Creating a new organization...");
            if(string.IsNullOrEmpty(organization.Name))
            {
                ModelState.AddModelError(nameof(Organization.Name), "Name is required");

                _logger.LogInformation("Returning organization creation view with error...");
                return View(organization);
            }

            organization.GDFolderId = await _googleDriveService.CreateFolderAsync(organization.Name, "");

            await _organizationService.AddAsync(organization);

            _logger.LogInformation("Returning next view, timeframes selection view...");
            return RedirectToAction(nameof(TimeframesIndex), new {id = organization.Id});
        }

        public async Task<IActionResult> TimeframesIndex(string id)
        {
            _logger.LogInformation("Moving to the timeframes step...");
            var timeframes = await _timeframeService.GetTimeframesByOrganizationId(id);

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

        public IActionResult TimeframeCreate(string organizationId)
        {
            _logger.LogInformation("A new timeframe needs to be created...");

            var timeframe = new Timeframe()
            {
                OrganizationId = Guid.Parse(organizationId)
            };

            _logger.LogInformation("Returning timeframes creation view...");
            return View(timeframe);
        }

        [HttpPost]
        public async Task<IActionResult> TimeframeCreate([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfProjects))] Timeframe timeframe)
        {
            _logger.LogInformation("Creating a new timeframe...");
            if (string.IsNullOrEmpty(timeframe.Name))
            {
                ModelState.AddModelError(nameof(Timeframe.Name), "Name is required");

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframe);
            }

            if(timeframe.NoOfProjects <= 0)
            {
                ModelState.AddModelError(nameof(Timeframe.NoOfProjects), "Number of projects must be greater than 0");

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframe);
            }

            var organizationFolderId = (await _organizationService.GetByIdAsync(timeframe.OrganizationId)).GDFolderId;
            timeframe.GDFolderId = await _googleDriveService.CreateFolderAsync(timeframe.Name, organizationFolderId);

            await _timeframeService.AddAsync(timeframe);

            _logger.LogInformation("Returning next view, timeframes selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id});
        }

        public async Task<IActionResult> ProjectsIndex(string organizationId, int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");
            var projects = await _projectService.GetProjectsByTimeframeId(organizationId, timeframeId);

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }
    }
}
