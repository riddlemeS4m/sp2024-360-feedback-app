using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
    [Route("{organizationId}/[controller]/[action]")]
    public class UploadProcessController : Controller
    {
        [FromRoute]
        public string OrganizationId { get; set; }
        public const string Name = "UploadProcess";
        private readonly IManageFeedback _manager;
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly ILogger<UploadProcessController> _logger;
        public UploadProcessController(
            IManageFeedback manager,
            IFeedbackDbServiceFactory serviceFactory,
            ILogger<UploadProcessController> logger) 
        { 
            _manager = manager;
            _dbServiceFactory = serviceFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting upload process...");

            var organizations = await _dbServiceFactory.OrganizationService.GetAllAsync();

            _logger.LogInformation("Returning organization selection view...");
            return View(organizations);
        }

        public async Task<IActionResult> TimeframesIndex()
        {
            _logger.LogInformation("Moving to the timeframes step...");
            var timeframes = await _dbServiceFactory.TimeframeService.GetTimeframesByOrganizationId(this.OrganizationId);

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

        public IActionResult TimeframeCreate()
        {
            _logger.LogInformation("A new timeframe needs to be created...");

            var timeframeVM = new TimeframeCreateVM()
            {
                OrganizationId = Guid.Parse(this.OrganizationId)
            };

            _logger.LogInformation("Returning timeframes creation view...");
            return View(timeframeVM);
        }

        [HttpPost]
        public async Task<IActionResult> TimeframeCreate([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfProjects),nameof(Timeframe.NoOfRounds))] Timeframe timeframe, 
            IEnumerable<string> ProjectNames)
        {
            _logger.LogInformation("Creating a new timeframe...");
            if (string.IsNullOrEmpty(timeframe.Name))
            {
                ModelState.AddModelError(nameof(TimeframeCreateVM.Name), "Name is required");

                var timeframeVM = new TimeframeCreateVM()
                {
                    OrganizationId = timeframe.OrganizationId,
                };

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframeVM);
            }

            if(timeframe.NoOfProjects < 0)
            {
                ModelState.AddModelError(nameof(Timeframe.NoOfProjects), "Number of projects must be greater than or equal to 0");

                var timeframeVM = new TimeframeCreateVM()
                {
                    OrganizationId = timeframe.OrganizationId,
                };

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframeVM);
            }

            await _manager.CreateTimeframe(timeframe, ProjectNames.ToList());

            _logger.LogInformation("Returning next view, projects selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id});
        }

        public async Task<IActionResult> ProjectsIndex(int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");
            var projects = await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId);

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        public async Task<IActionResult> ProjectRoundCreate(int timeframeId)
        {
            _logger.LogInformation("Project rounds need to be created...");
            var timeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframeId);
            if(timeframe.NoOfRounds > 0)
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(new Project
                {
                    NoOfRounds = timeframe.NoOfRounds
                });
            }

            var projects = await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId);
            if(projects.Count() >  0) 
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(projects.ElementAt(0));
            }

            _logger.LogInformation("Returning rounds creation view...");
            return View(new Project { NoOfRounds = 0 });
        }

        [HttpPost]
        public async Task<IActionResult> ProjectRoundCreate([Bind(nameof(Project.Id),nameof(Project.OrganizationId),nameof(Project.TimeframeId),nameof(Project.NoOfRounds))] Project project, 
            List<DateTime> RoundStartDates, List<DateTime> RoundEndDates)
        {
            _logger.LogInformation("Creating project rounds...");

            await _manager.CreateProjectRounds(project, RoundStartDates, RoundEndDates);

            _logger.LogInformation("Returning to project selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = project.OrganizationId, timeframeId = project.TimeframeId });
        }

        public async Task<IActionResult> ProjectRoundsIndex(int timeframeId, string projectId)
        {
            _logger.LogInformation("Moving to the rounds step...");
            var projectRounds = await _dbServiceFactory.ProjectRoundService.GetProjectRoundsByProjectId(projectId);

            var vm = new ProjectRoundsIndexVM
            {
                ProjectRounds = projectRounds,
                OrganizationId = this.OrganizationId,
                TimeframeId = timeframeId,
            };

            _logger.LogInformation("Returning rounds selection view...");
            return View(vm);
        }

        // public async Task<IActionResult> FeedbackIndex(string projectId, int timeframeId, int roundId)
        // {
        //     return RedirectToAction(nameof(CreatePdfs), new { timeframeId, roundId });
        // }

        public async Task<IActionResult> FeedbackPdfsIndex(int timeframeId, string projectId, int roundId)
        {
            var pdfs = await _dbServiceFactory.FeedbackPdfService.GetFeedbackByProjectIdAndRoundId(Guid.Parse(projectId), roundId);

            return View(pdfs);
        }

        public async Task<IActionResult> RouteUploadRoster(int timeframeId)
        {
            var orgType = (await _dbServiceFactory.OrganizationService.GetByIdAsync(Guid.Parse(OrganizationId))).Type;
            return RedirectToAction(nameof(BaseController.UploadRoster), orgType, new { area = orgType, organizationId = OrganizationId, timeframeId = timeframeId });
        }
    }
}
